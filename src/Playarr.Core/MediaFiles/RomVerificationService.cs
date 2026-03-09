using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;
using NLog;

namespace Playarr.Core.MediaFiles
{
    public interface IRomVerificationService
    {
        string ComputeCrc(string filePath);
        RomVerificationResult Verify(string crcHash);
        void LoadDatFile(string datFilePath);
        int LoadedEntryCount { get; }
    }

    public enum RomVerificationResult
    {
        Unknown,
        Bad,
        Verified
    }

    public class RomVerificationService : IRomVerificationService
    {
        private readonly Logger _logger;
        private readonly HashSet<string> _knownCrcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public int LoadedEntryCount => _knownCrcs.Count;

        public RomVerificationService(Logger logger)
        {
            _logger = logger;
        }

        public string ComputeCrc(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _logger.Warn("Cannot compute CRC for non-existent file: {0}", filePath);
                return null;
            }

            try
            {
                using (var stream = File.OpenRead(filePath))
                using (var crc32 = new Crc32())
                {
                    var hash = crc32.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to compute CRC for file: {0}", filePath);
                return null;
            }
        }

        public RomVerificationResult Verify(string crcHash)
        {
            if (string.IsNullOrWhiteSpace(crcHash))
            {
                return RomVerificationResult.Unknown;
            }

            if (_knownCrcs.Count == 0)
            {
                return RomVerificationResult.Unknown;
            }

            return _knownCrcs.Contains(crcHash)
                ? RomVerificationResult.Verified
                : RomVerificationResult.Bad;
        }

        public void LoadDatFile(string datFilePath)
        {
            if (!File.Exists(datFilePath))
            {
                _logger.Warn("DAT file not found: {0}", datFilePath);
                return;
            }

            try
            {
                var doc = XDocument.Load(datFilePath);
                var root = doc.Root;

                if (root == null)
                {
                    return;
                }

                // Support both No-Intro and Redump DAT XML formats
                // No-Intro: <datafile><game><rom name="..." size="..." crc="..." md5="..." sha1="..."/></game></datafile>
                // Redump: <datafile><game><rom name="..." size="..." crc="..." md5="..." sha1="..."/></game></datafile>
                // CLRMAMEPro: <datafile><game><rom name="..." size="..." crc="..." md5="..." sha1="..."/></game></datafile>
                var games = root.Elements("game");

                foreach (var game in games)
                {
                    foreach (var rom in game.Elements("rom"))
                    {
                        var crc = rom.Attribute("crc")?.Value;

                        if (!string.IsNullOrWhiteSpace(crc))
                        {
                            _knownCrcs.Add(crc.ToUpperInvariant());
                        }
                    }
                }

                _logger.Info("Loaded {0} CRC entries from DAT file: {1}", _knownCrcs.Count, datFilePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load DAT file: {0}", datFilePath);
            }
        }
    }

    /// <summary>
    /// CRC-32 implementation for ROM verification (IEEE 802.3 polynomial).
    /// </summary>
    internal sealed class Crc32 : HashAlgorithm
    {
        private static readonly uint[] Table = GenerateTable();
        private uint _crc = 0xFFFFFFFF;

        public override int HashSize => 32;

        private static uint[] GenerateTable()
        {
            var table = new uint[256];

            for (uint i = 0; i < 256; i++)
            {
                var entry = i;
                for (var j = 0; j < 8; j++)
                {
                    entry = (entry & 1) != 0
                        ? (entry >> 1) ^ 0xEDB88320
                        : entry >> 1;
                }

                table[i] = entry;
            }

            return table;
        }

        public override void Initialize()
        {
            _crc = 0xFFFFFFFF;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for (var i = ibStart; i < ibStart + cbSize; i++)
            {
                _crc = Table[(_crc ^ array[i]) & 0xFF] ^ (_crc >> 8);
            }
        }

        protected override byte[] HashFinal()
        {
            _crc ^= 0xFFFFFFFF;
            return new[]
            {
                (byte)((_crc >> 24) & 0xFF),
                (byte)((_crc >> 16) & 0xFF),
                (byte)((_crc >> 8) & 0xFF),
                (byte)(_crc & 0xFF)
            };
        }
    }
}

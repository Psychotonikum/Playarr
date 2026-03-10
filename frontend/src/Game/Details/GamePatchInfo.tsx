import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import styles from './GamePatchInfo.css';

interface PatchFile {
  fileName: string;
  fileType: number;
  version?: string;
  dlcIndex?: string;
}

interface GamePatchInfoProps {
  systemType: number;
  baseFile?: PatchFile;
  updates: PatchFile[];
  dlcs: PatchFile[];
  isMissingBase: boolean;
}

function GamePatchInfo({
  systemType,
  baseFile,
  updates,
  dlcs,
  isMissingBase,
}: GamePatchInfoProps) {
  // Only show for patchable systems
  if (systemType !== 1) {
    return null;
  }

  const activeUpdate =
    updates.length > 0
      ? updates.reduce((a, b) => {
          const aVer = parseInt(a.version ?? '0', 10);
          const bVer = parseInt(b.version ?? '0', 10);
          return bVer > aVer ? b : a;
        })
      : null;

  return (
    <div className={styles.patchInfo}>
      <div className={styles.patchInfoHeader}>Game Files</div>

      <div className={styles.patchRow}>
        <span className={styles.patchLabel}>Base:</span>
        {baseFile ? (
          <Label kind={kinds.SUCCESS}>Present</Label>
        ) : (
          <Label kind={kinds.DANGER}>Missing</Label>
        )}
      </div>

      <div className={styles.patchRow}>
        <span className={styles.patchLabel}>Update:</span>
        {activeUpdate ? (
          <Label kind={kinds.SUCCESS}>v{activeUpdate.version}</Label>
        ) : (
          <span className={styles.version}>None</span>
        )}
      </div>

      {dlcs.length > 0 ? (
        <div className={styles.patchRow}>
          <span className={styles.patchLabel}>DLC:</span>
          <div className={styles.dlcList}>
            {dlcs.map((dlc) => (
              <Label key={dlc.dlcIndex} kind={kinds.INFO}>
                DLC{dlc.dlcIndex}
              </Label>
            ))}
          </div>
        </div>
      ) : (
        <div className={styles.patchRow}>
          <span className={styles.patchLabel}>DLC:</span>
          <span className={styles.version}>None</span>
        </div>
      )}

      {isMissingBase ? (
        <div className={styles.orphanWarning}>
          Warning: Updates/DLC found but base game is missing
        </div>
      ) : null}
    </div>
  );
}

export default GamePatchInfo;

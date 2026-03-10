import React, { useCallback, useEffect, useState } from 'react';
import { toggleIsSidebarVisible } from 'App/appStore';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import GameSearchInput from './GameSearchInput';
import KeyboardShortcutsModal from './KeyboardShortcutsModal';
import PageHeaderActionsMenu from './PageHeaderActionsMenu';
import styles from './PageHeader.css';

function PageHeader() {
  const [isKeyboardShortcutsModalOpen, setIsKeyboardShortcutsModalOpen] =
    useState(false);

  const { bindShortcut, unbindShortcut } = useKeyboardShortcuts();

  const handleSidebarToggle = useCallback(() => {
    toggleIsSidebarVisible();
  }, []);

  const handleOpenKeyboardShortcutsModal = useCallback(() => {
    setIsKeyboardShortcutsModalOpen(true);
  }, []);

  const handleKeyboardShortcutsModalClose = useCallback(() => {
    setIsKeyboardShortcutsModalOpen(false);
  }, []);

  useEffect(() => {
    bindShortcut(
      'openKeyboardShortcutsModal',
      handleOpenKeyboardShortcutsModal
    );

    return () => {
      unbindShortcut('openKeyboardShortcutsModal');
    };
  }, [handleOpenKeyboardShortcutsModal, bindShortcut, unbindShortcut]);

  return (
    <div className={styles.header}>
      <div className={styles.logoContainer}>
        <Link className={styles.logoLink} to="/">
          <img
            className={styles.logo}
            src={`${window.Playarr.urlBase}/Content/Images/logo.svg`}
            alt="Playarr Logo"
          />
        </Link>
      </div>

      <div className={styles.sidebarToggleContainer}>
        <IconButton
          id="sidebar-toggle-button"
          name={icons.NAVBAR_COLLAPSE}
          onPress={handleSidebarToggle}
        />
      </div>

      <GameSearchInput />

      <div className={styles.right}>
        <IconButton
          className={styles.donate}
          name={icons.HEART}
          aria-label={translate('Donate')}
          to="https://github.com/Psychotonikum/Playarr"
          size={14}
          title={translate('Donate')}
        />

        <PageHeaderActionsMenu
          onKeyboardShortcutsPress={handleOpenKeyboardShortcutsModal}
        />
      </div>

      <KeyboardShortcutsModal
        isOpen={isKeyboardShortcutsModalOpen}
        onModalClose={handleKeyboardShortcutsModalClose}
      />
    </div>
  );
}

export default PageHeader;

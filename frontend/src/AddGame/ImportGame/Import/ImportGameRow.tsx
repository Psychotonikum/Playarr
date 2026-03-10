import React, { useCallback, useEffect } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import FormInputGroup from 'Components/Form/FormInputGroup';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import useExistingGame from 'Game/useExistingGame';
import { inputTypes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import {
  ImportGameItem,
  UnamppedFolderItem,
  updateImportGameItem,
  useImportGameItem,
} from './importGameStore';
import ImportGameSelectGame from './SelectGame/ImportGameSelectGame';
import styles from './ImportGameRow.css';

interface ImportGameRowProps {
  unmappedFolder: UnamppedFolderItem;
}

function ImportGameRow({ unmappedFolder }: ImportGameRowProps) {
  const id = unmappedFolder.id;

  const item = useImportGameItem(unmappedFolder.id);

  const { relativePath, monitor, selectedSeries } = item ?? {};

  const isExistingSeries = useExistingGame(selectedSeries?.igdbId);

  const { getIsSelected, toggleSelected, toggleDisabled } =
    useSelect<ImportGameItem>();

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      updateImportGameItem({ id, [name]: value });
    },
    [id]
  );

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps<string>) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [toggleSelected]
  );

  useEffect(() => {
    toggleDisabled(id, !selectedSeries || isExistingSeries);
  }, [id, selectedSeries, isExistingSeries, toggleDisabled]);

  useEffect(() => {
    toggleSelected({ id, isSelected: !!selectedSeries, shiftKey: false });
  }, [id, selectedSeries, toggleSelected]);

  return (
    <>
      <VirtualTableSelectCell<string>
        inputClassName={styles.selectInput}
        id={id}
        isSelected={getIsSelected(id)}
        isDisabled={!selectedSeries || isExistingSeries}
        onSelectedChange={handleSelectedChange}
      />

      <VirtualTableRowCell className={styles.folder}>
        {relativePath}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.monitor}>
        <FormInputGroup
          type={inputTypes.MONITOR_EPISODES_SELECT}
          name="monitor"
          value={monitor}
          onChange={handleInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.game}>
        <ImportGameSelectGame id={id} />
      </VirtualTableRowCell>
    </>
  );
}

export default ImportGameRow;

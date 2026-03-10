import React from 'react';
import Label from 'Components/Label';
import { useSingleGame } from 'Game/useGame';
import { kinds, sizes } from 'Helpers/Props';
import { useTagList } from 'Tags/useTags';
import sortByProp from 'Utilities/Array/sortByProp';

interface GameTagsProps {
  gameId: number;
}

function GameTags({ gameId }: GameTagsProps) {
  const game = useSingleGame(gameId)!;
  const tagList = useTagList();

  const tags = game.tags
    .map((tagId) => tagList.find((tag) => tag.id === tagId))
    .filter((tag) => !!tag)
    .sort(sortByProp('label'))
    .map((tag) => tag.label);

  return (
    <div>
      {tags.map((tag) => {
        return (
          <Label key={tag} kind={kinds.INFO} size={sizes.LARGE}>
            {tag}
          </Label>
        );
      })}
    </div>
  );
}

export default GameTags;

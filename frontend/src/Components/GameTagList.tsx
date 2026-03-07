import React from 'react';
import { useTagList } from 'Tags/useTags';
import TagList from './TagList';

interface GameTagListProps {
  tags: number[];
}

function GameTagList({ tags }: GameTagListProps) {
  const tagList = useTagList();

  return <TagList tags={tags} tagList={tagList} />;
}

export default GameTagList;

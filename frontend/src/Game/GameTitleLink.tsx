import React from 'react';
import Link, { LinkProps } from 'Components/Link/Link';

export interface GameTitleLinkProps extends LinkProps {
  titleSlug: string;
  title: string;
}

export default function GameTitleLink({
  titleSlug,
  title,
  ...linkProps
}: GameTitleLinkProps) {
  const link = `/game/${titleSlug}`;

  return (
    <Link to={link} {...linkProps}>
      {title}
    </Link>
  );
}

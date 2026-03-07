import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import FieldSet from 'Components/FieldSet';
import Link from 'Components/Link/Link';
import translate from 'Utilities/String/translate';

function MoreInfo() {
  return (
    <FieldSet legend={translate('MoreInfo')}>
      <DescriptionList>
        <DescriptionListItemTitle>
          {translate('HomePage')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://playarr.tv/">playarr.tv</Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>{translate('Wiki')}</DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://wiki.servarr.com/playarr">
            wiki.servarr.com/playarr
          </Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>
          {translate('Forums')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://forums.playarr.tv/">forums.playarr.tv</Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>
          {translate('Twitter')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://twitter.com/playarrtv">@playarrtv</Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>
          {translate('Discord')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://discord.playarr.tv/">discord.playarr.tv</Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>{translate('IRC')}</DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="irc://irc.libera.chat/#playarr">
            {translate('IRCLinkText')}
          </Link>
        </DescriptionListItemDescription>
        <DescriptionListItemDescription>
          <Link to="https://web.libera.chat/?channels=#playarr">
            {translate('LiberaWebchat')}
          </Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>
          {translate('Donations')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://playarr.tv/donate">playarr.tv/donate</Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>
          {translate('Source')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://github.com/Playarr/Playarr/">
            github.com/Playarr/Playarr
          </Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>
          {translate('FeatureRequests')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://forums.playarr.tv/">forums.playarr.tv</Link>
        </DescriptionListItemDescription>
        <DescriptionListItemDescription>
          <Link to="https://github.com/Playarr/Playarr/issues">
            github.com/Playarr/Playarr/issues
          </Link>
        </DescriptionListItemDescription>
      </DescriptionList>
    </FieldSet>
  );
}

export default MoreInfo;

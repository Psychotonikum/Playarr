import moment from 'moment';
import React from 'react';
import useCalendar from 'Calendar/useCalendar';
import useUpcomingReleases from 'Calendar/useUpcomingReleases';
import AgendaEvent from './AgendaEvent';
import UpcomingAgendaEvent from './UpcomingAgendaEvent';
import styles from './Agenda.css';

interface AgendaItem {
  type: 'library' | 'upcoming';
  date: string;
  data: unknown;
}

function Agenda() {
  const { data: libraryData } = useCalendar();
  const { data: upcomingData } = useUpcomingReleases();

  const items: AgendaItem[] = [
    ...libraryData.map((item) => ({
      type: 'library' as const,
      date: item.airDateUtc,
      data: item,
    })),
    ...upcomingData
      .filter((r) => r.releaseDate)
      .map((release) => ({
        type: 'upcoming' as const,
        date: release.releaseDate!,
        data: release,
      })),
  ].sort((a, b) => moment(a.date).unix() - moment(b.date).unix());

  return (
    <div className={styles.agenda}>
      {items.map((item, index) => {
        const momentDate = moment(item.date);
        const showDate =
          index === 0 ||
          !moment(items[index - 1].date).isSame(momentDate, 'day');

        if (item.type === 'upcoming') {
          const release = item.data as (typeof upcomingData)[0];
          return (
            <UpcomingAgendaEvent
              key={`upcoming-${release.igdbId}-${release.title}`}
              release={release}
              showDate={showDate}
            />
          );
        }

        const calendarItem = item.data as (typeof libraryData)[0];
        return (
          <AgendaEvent
            key={calendarItem.id}
            showDate={showDate}
            {...calendarItem}
          />
        );
      })}
    </div>
  );
}

export default Agenda;

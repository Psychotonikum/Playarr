import Rom from 'Rom/Rom';

export interface CalendarItem extends Omit<Rom, 'airDateUtc'> {
  airDateUtc: string;
}

export interface CalendarEvent extends CalendarItem {
  isGroup: false;
}

export interface CalendarEventGroup {
  isGroup: true;
  gameId: number;
  platformNumber: number;
  romIds: number[];
  events: CalendarItem[];
}

export type CalendarStatus =
  | 'downloaded'
  | 'downloading'
  | 'unmonitored'
  | 'onAir'
  | 'missing'
  | 'unaired';

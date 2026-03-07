interface EpisodeSearchPayload {
  romId: number;
}

interface SeasonSearchPayload {
  gameId: number;
  platformNumber: number;
}

type InteractiveSearchPayload = EpisodeSearchPayload | SeasonSearchPayload;

export default InteractiveSearchPayload;

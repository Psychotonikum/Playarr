import { useQualityProfile } from 'Settings/Profiles/Quality/useQualityProfiles';
import Game from './Game';

const useGameQualityProfile = (game: Game | undefined) => {
  return useQualityProfile(game?.qualityProfileId);
};

export default useGameQualityProfile;

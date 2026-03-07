import Game from 'Game/Game';

interface AddGame extends Game {
  folder: string;
  isExcluded: boolean;
}

export default AddGame;

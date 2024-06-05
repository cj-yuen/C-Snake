namespace C_Snake
{
    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakeBody = new LinkedList<Position>();
        private readonly Random random = new Random();

        /*
         *  Constructor 
         */
        public GameState(int rows, int cols)
        {
            Rows = rows; 
            Cols = cols;
            Grid = new GridValue[Rows, Cols];
            Dir = Direction.Right;

            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int midRow = Rows / 2;

            for (int i = 0; i < 3; i++) // snake start w/3 cells
            {
                Grid[midRow, i] = GridValue.Snake;
                snakeBody.AddFirst(new Position(midRow, i));
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (Grid[i, j] == GridValue.Empty)
                    {
                        yield return new Position(i, j);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());
            
            // Check if you "beat" the game
            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }

        public Position HeadPosition()
        {
           return snakeBody.First.Value;
        }

        public Position TailPosition()
        {
            return snakeBody.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakeBody;
        }

        private void AddHead(Position pos)
        {
            snakeBody.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakeBody.Last.Value; 
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakeBody.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir;
            }

            return dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }

        private bool OutsideGride(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        private GridValue WillHit(Position newHeadPos)
        {
            if (OutsideGride(newHeadPos))
            {
                return GridValue.Outside;
            }

            // if head passes right by its tail 
            if (newHeadPos == TailPosition())
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {
            if (dirChanges.Count > 0)
            {
               Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);
            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if (hit == GridValue.Food) {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }
        }
    }
}
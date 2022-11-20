using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace ParallelLab3.ViewModels
{
    public class MainViewModel : Screen, IHandle<int>
    {
        private List<CanvasPoint> _points = new List<CanvasPoint>();

        public List<CanvasPoint> Points
        {
            get
            {
                lock (this)
                {
                    return _points; 
                }
            }

            set
            {
                lock (this)
                {

                    _points = value; 
                }
            }
        }

        private GameOfLife _gameOfLife;

        private IEventAggregator _eventAggregator;

        public MainViewModel(IEventAggregator eventAgg)
        {
            _eventAggregator = eventAgg;
            _eventAggregator.SubscribeOnUIThread(this);
            _gameOfLife = new GameOfLife();
            Task.Run(async () =>
            {
                var sw = new Stopwatch();

                while (true)
                {
                    await _gameOfLife.MakeStep();

                    var el1 = sw.Elapsed;

                    lock (this)
                    {
                        lock (_gameOfLife)
                        {
                            _points = new List<CanvasPoint>();

                            for (int i = 0; i < 60; i++)
                            {
                                for (int j = 0; j < 60; j++)
                                {
                                    if (_gameOfLife.CurrentGeneration[i, j] == 1)
                                    {
                                        _points.Add(new CanvasPoint { X = i * 10, Y = j * 10 });
                                    }
                                }
                            }  
                        }
                    }

                    sw.Restart();
                    await _eventAggregator.PublishOnUIThreadAsync(1);
                    sw.Stop();

                    var el3 = sw.Elapsed;

                }
            });

        }

        public Task HandleAsync(int message, CancellationToken cancellationToken)
        {
            lock (this)
            {
                NotifyOfPropertyChange(nameof(Points));
            }

            return Task.CompletedTask;
        }
    }

    public class GameOfLife
    {
        public int[,] CurrentGeneration { get; private set; }

        private int[,] nextGeneration;

        private int _x, _y;

        public GameOfLife()
        {
            _x = 60;
            _y = 60;

            CurrentGeneration = new int[_x, _y];
            nextGeneration = new int[_x, _y];

            // Cycle cells using rng to set live/dead cells
            var random = new Random();
            for (int i = 0; i < _x; i++)
            {
                for (int j = 0; j < _y; j++)
                {
                    // Random Board
                    if (random.Next(1, 100) < 99)
                    {
                        CurrentGeneration[i, j] = 0;
                    }
                    else
                    {
                        CurrentGeneration[i, j] = 1;
                    }
                }
            }
        }

        public async Task MakeStep()
        {
            /*
            Any live cell with fewer than two live neighbours dies, as if by underpopulation.
            Any live cell with two or three live neighbours lives on to the next generation.
            Any live cell with more than three live neighbours dies, as if by overpopulation.
            Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
            */

            var range = new List<(int x, int y)>();
            for (int i = 0; i < _x; i++)
            {
                for (int j = 0; j < _y; j++)
                { 
                    range.Add((i, j)); 
                }
            }

            Parallel.ForEach(range, index =>
            {
                var liveNeighbours = CountLiveNeighbours(index.x, index.y, CurrentGeneration);
                KillIfLessThanTwoLiveNeighbours(index.x, index.y, liveNeighbours);
                LiveOnIfTwoOrThreeLiveNeighbours(index.x, index.y, liveNeighbours);
                KillIfMoreThanThreeLiveNeighbours(index.x, index.y, liveNeighbours);
                ResurectIfThreeLiveNeighbours(index.x, index.y, liveNeighbours);
            });
            lock (this)
            {
                CurrentGeneration = nextGeneration;
                nextGeneration = new int[_x, _y];
            }


            //await Task.Run(() =>
            //{
            //    for (int i = 0; i < _x; i++)
            //    {
            //        for (int j = 0; j < _y; j++)
            //        {
            //            var liveNeighbours = CountLiveNeighbours(i, j, CurrentGeneration);
            //            KillIfLessThanTwoLiveNeighbours(i, j, CurrentGeneration, liveNeighbours);
            //            LiveOnIfTwoOrThreeLiveNeighbours(i, j, CurrentGeneration, liveNeighbours);
            //            KillIfMoreThanThreeLiveNeighbours(i, j, CurrentGeneration, liveNeighbours);
            //            ResurectIfThreeLiveNeighbours(i, j, CurrentGeneration, liveNeighbours);
            //        }
            //    }
            //    CurrentGeneration = nextGeneration;
            //    nextGeneration = new int[_x, _y];
            //}); 

        }

        private void KillIfLessThanTwoLiveNeighbours(int i, int j, int liveNeighbours) 
        {
            if (liveNeighbours < 2)
            {
                nextGeneration[i, j] = 0;
            }
        }

        private void LiveOnIfTwoOrThreeLiveNeighbours(int i, int j, int liveNeighbours)
        {
            if (liveNeighbours == 3 || liveNeighbours == 2)
            {
                nextGeneration[i, j] = 1;
            }
        }

        private void KillIfMoreThanThreeLiveNeighbours(int i, int j, int liveNeighbours)
        {
            if (liveNeighbours > 3)
            {
                nextGeneration[i, j] = 0;
            }
        }

        private void ResurectIfThreeLiveNeighbours(int i, int j, int liveNeighbours)
        {
            if (liveNeighbours == 3)
            {
                nextGeneration[i, j] = 1;
            }
        }

        private int CountLiveNeighbours(int i, int j, int[,] array)
        {
            int liveNeighboursCount = 0;

            (int, int)[] neigbours = { (i - 1, j - 1), (i, j - 1), (i + 1, j - 1),
                                       (i - 1, j), /*    me     */ (i + 1, j),
                                       (i - 1, j + 1), (i, j + 1), (i + 1, j + 1) };

            foreach (var neigbour in neigbours)
            {
                if (neigbour.Item1 < 0 || neigbour.Item1 > _x - 1 || neigbour.Item2 < 0 || neigbour.Item2 > _y - 1)
                {
                    // Out of bounds.
                    continue;
                }

                if (array[neigbour.Item1, neigbour.Item2] == 1)
                {
                    liveNeighboursCount++;
                }
            }

            return liveNeighboursCount;
        }
    }


    public struct CanvasPoint
    {
        private int _x;

        public int X
        {
            get 
            { 
                return _x; 
            }
            set 
            { 
                _x = value; 
            }
        }

        private int _y;

        public int Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

    }
}

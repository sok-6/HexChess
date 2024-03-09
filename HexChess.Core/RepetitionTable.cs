using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public class RepetitionTable
    {
        private Dictionary<ulong, byte> _data;

        public RepetitionTable()
        {
            _data = new Dictionary<ulong, byte>();
        }

        private RepetitionTable(Dictionary<ulong, byte> parentData)
        {
            _data = new Dictionary<ulong, byte>(parentData);
        }

        public RepetitionTable Clone()
        {
            return new RepetitionTable(_data);
        }

        /// <summary>
        /// Adds a visit to a position
        /// </summary>
        /// <param name="positionHash">The hash of the position to visit</param>
        /// <returns>The number of times this position has come up</returns>
        public int AddVisit(ulong positionHash)
        {
            if (_data.ContainsKey(positionHash))
            {
                var visits = ++_data[positionHash];

                Debug.WriteLine("Repetition Table:");
                Debug.WriteLine(String.Join(Environment.NewLine, _data.Select(k => $"{k.Key}-{k.Value}")));

                return visits;
            }
            else
            {
                _data.Add(positionHash, 1);

                Debug.WriteLine("Repetition Table:");
                Debug.WriteLine(String.Join(Environment.NewLine, _data.Select(k => $"{k.Key}-{k.Value}")));

                return 1;
            }
        }

        public int GetVisitCount(ulong positionHash)
        {
            _data.TryGetValue(positionHash, out var visits);
            return visits;
        }

        public void RemoveVisit(ulong positionHash)
        {
            if (_data.TryGetValue(positionHash, out var visits))
            {
                visits--;
                if (visits == 0)
                {
                    _data.Remove(positionHash);
                }
                else
                {
                    _data[positionHash] = visits;
                }
            }
            else
            {
                // If trying to remove a position that hasn't come up yet something's gone wrong
                throw new Exception("Can't remove position that hasn't come up yet");
            }

            Debug.WriteLine("Repetition Table:");
            Debug.WriteLine(String.Join(Environment.NewLine, _data.Select(k => $"{k.Key}-{k.Value}")));
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class PreMove
    {
        public PieceID responsibleColor;
        
        public PreMove(PieceID responsibleColor)
        {
            this.responsibleColor = responsibleColor;
        }

        public List<Move> legalMoves= new List<Move>();
        public List<int> trueLegalSources = new List<int>();
        public List<int> trueLegalTargets = new List<int>();

    }
}

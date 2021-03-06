﻿

using System;

namespace Dvonn_Console
{
    public enum directionID
    {
        NE, EA, SE, SW, WE, NW
    }

    public enum GamePhase
    {
        EarlyGame, Apex, PostApex, EndGame
    }

    public enum PieceID
    {
        Dvonn, White, Black
    }

    public static class PieceIDExtensions
    {
        public static char ToChar(this PieceID id)
        {
            if (id == PieceID.Dvonn) return 'D';
            if (id == PieceID.White) return 'W';
            if (id == PieceID.Black) return 'B';

            throw new ArgumentException("Unexpected piece id received: " + id.ToString());

        }

        public static PieceID ToOpposite(this PieceID id)
        {
            if (id == PieceID.White) return PieceID.Black;
            if (id == PieceID.Black) return PieceID.White;

            throw new ArgumentException("Unexpected piece id received: " + id.ToString());

        }

        public static PieceID ToPieceID(this string str)
        {
            if (str == "W") return PieceID.White;
            if (str == "B") return PieceID.Black;
            if (str == "D") return PieceID.Dvonn;

            throw new ArgumentException("Unexpected char received: " + str.ToString());

        }

    }

}

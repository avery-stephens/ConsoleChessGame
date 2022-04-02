﻿using System;
using XadrezDeConsole.Domain.Abstraction;
using XadrezDeConsole.Domain.Entities;

namespace XadrezDeConsole.Helpers
{
    public class Screen
    {
        public static void PrintBoard(Board board, Match match)
        {
            foreach(var piece in match.GetYellowCapturedPieces())
            {
                PrintPiece(piece);
            }

            Console.WriteLine();
            Console.WriteLine();

            for (int i = 0; i < board.Lines; i++)
            {
                Console.Write(board.Columns - i + "  ");
                for (int j = 0; j < board.Columns; j++)
                {
                    PrintPiece(board.Piece(i, j));
                }
                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("    a   b   c   d   e   f   g   h");

            Console.WriteLine();

            foreach (var piece in match.GetRedCapturedPieces())
            {
                PrintPiece(piece);
            }

            Console.WriteLine();
            match.VerifyCheckmate(match.CurrentPlayer);
            if (match.check)
            {
                Console.WriteLine("You can't put yourself in check, try again!");
            }
            Console.WriteLine();
        }

        public static void PrintBoard(Board board, bool[,] possibleMovements)
        {
            for (int i = 0; i < board.Lines; i++)
            {
                Console.Write(board.Columns - i + "  ");
                for (int j = 0; j < board.Columns; j++)
                {
                    if (possibleMovements[i, j] == true)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                    }
                    PrintPiece(board.Piece(i, j));
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("    a   b   c   d   e   f   g   h");
        }


        public static void PrintPiece(Piece piece)
        {
            if (piece == null)
            {
                Console.Write(" -  ");
            }
            else
            {
                if (piece != null)
                {
                    Console.ForegroundColor = (ConsoleColor)piece.Color;
                }

                Console.Write(piece);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" ");
            }
        }

        public static ScreenPositon ReadPosition()
        {
            var movement = Console.ReadLine();
            return new ScreenPositon(movement[0], Convert.ToInt32(movement[1].ToString()));
        }
    }
}

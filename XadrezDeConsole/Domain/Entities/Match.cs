﻿using System;
using System.Collections.Generic;
using System.Linq;
using XadrezDeConsole.Domain.Abstraction;
using XadrezDeConsole.Helpers;
using XadrezDeConsole.Helpers.Enums;
using XadrezDeConsole.Helpers.GameException;

namespace XadrezDeConsole.Domain.Entities
{
    public class Match
    {
        public Board Board { get; private set; }
        private int Turn;
        public ChessMove check { get; private set; }
        public Color CurrentPlayer { get; private set; }
        public bool IsFinished { get; private set; }
        public Position CastlingPosition { get; set; }
        public Piece EnPassant { get; set; }
        public bool EnPassantAllowed { get; set; }
        public HashSet<Piece> PossiblePieces { get; private set; }
        public HashSet<Piece> MatchPieces { get; private set; }
        private HashSet<Piece> CapturedPieces;

        public Match()
        {
            this.Board = new Board();
            this.Turn = 1;
            this.CurrentPlayer = Color.Yellow;
            this.MatchPieces = new HashSet<Piece>();
            this.CapturedPieces = new HashSet<Piece>();
            this.PossiblePieces = new HashSet<Piece>();
            this.check = ChessMove.None;
            SetBoard();
        }

        public void ExecuteMovement(Position origin, Position destination)
        {
            var piece = this.Board.Piece(origin);
            var possibleMovements = piece.PossibleMovements();

            if (possibleMovements[destination.Line, destination.Column])
            {
                var pieceTrapped = MovePiece(origin, destination);
                this.check = VerifyCheck(this.CurrentPlayer);
                switch (this.check)
                {
                    case ChessMove.Check:
                        MovePiece(destination, origin);
                        if (pieceTrapped != null)
                        {
                            this.Board.InsertPiece(pieceTrapped, destination);
                            this.MatchPieces.Add(pieceTrapped);
                            this.CapturedPieces.Remove(pieceTrapped);
                        }
                        break;

                    case ChessMove.Checkmate:
                        break;

                    case ChessMove.Stalemate:
                        break;

                    default:
                        if (CastlingPosition != null && (CastlingPosition?.Line == destination.Line && CastlingPosition?.Column == destination.Column))
                        {
                            CastlingMove(piece);
                        }

                        if (piece is Pawn && piece.Movements == 1)
                        {
                            this.EnPassant = piece;
                        }
                        else if (piece is Pawn && this.EnPassantAllowed)
                        {
                            IsEnPassantMove(piece, destination);
                        }

                        if (piece is Pawn && (destination.Line == 0 || destination.Line == 7))
                        {
                            VerifyPromotion(piece);
                        }

                        this.Turn++;
                        ChangePlayer();
                        this.check = VerifyCheck(this.CurrentPlayer);
                        break;
                }
            }
            else
            {
                throw new GameException($"Position Invalid: {destination}");
            }
        }

        public void ChangePlayer()
        {
            if (this.CurrentPlayer == Color.Yellow)
            {
                this.CurrentPlayer = Color.Red;
            }
            else
            {
                this.CurrentPlayer = Color.Yellow;
            }
        }

        public void ValidateOrigin(Position position)
        {
            VerifyStealmate(position);

            if (!this.Board.IsValidPosition(position) || this.Board.Piece(position)?.Color != this.CurrentPlayer)
            {
                throw new GameException($"Position Invalid: {position}");
            }

            if (this.Board.Piece(position) == null)
            {
                throw new GameException($"No Piece Selected: {position}");
            }
        }

        public Piece MovePiece(Position origin, Position destination)
        {
            Piece piece = this.Board.RemovePiece(origin);
            piece.Move();
            Piece pieceTrapped = this.Board.RemovePiece(destination);
            this.Board.InsertPiece(piece, destination);

            if (pieceTrapped != null)
            {
                this.CapturedPieces.Add(pieceTrapped);
                this.MatchPieces.Remove(pieceTrapped);
            }

            return pieceTrapped;
        }

        public void PlacePiece(Piece piece, Position position)
        {
            this.Board.InsertPiece(piece, position);
            this.MatchPieces.Add(piece);
            this.PossiblePieces.Add(piece);
        }

        private void SetBoard()
        {
            //yellow
            PlacePiece(new Rook(this.Board, Color.Yellow), new ScreenPositon('a', 1).ToPosition(this.Board));
            PlacePiece(new Knight(this.Board, Color.Yellow), new ScreenPositon('b', 1).ToPosition(this.Board));
            PlacePiece(new Bishop(this.Board, Color.Yellow), new ScreenPositon('c', 1).ToPosition(this.Board));
            PlacePiece(new King(this.Board, Color.Yellow, this), new ScreenPositon('e', 1).ToPosition(this.Board));
            PlacePiece(new Queen(this.Board, Color.Yellow), new ScreenPositon('d', 1).ToPosition(this.Board));
            PlacePiece(new Bishop(this.Board, Color.Yellow), new ScreenPositon('f', 1).ToPosition(this.Board));
            PlacePiece(new Knight(this.Board, Color.Yellow), new ScreenPositon('g', 1).ToPosition(this.Board));
            PlacePiece(new Rook(this.Board, Color.Yellow), new ScreenPositon('h', 1).ToPosition(this.Board));

            PlacePiece(new Pawn(this.Board, Color.Yellow, new Position(0, 2), this), new ScreenPositon('a', 2).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Yellow, new Position(1, 2), this), new ScreenPositon('b', 2).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Yellow, new Position(2, 2), this), new ScreenPositon('c', 2).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Yellow, new Position(3, 2), this), new ScreenPositon('d', 2).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Yellow, new Position(4, 2), this), new ScreenPositon('e', 2).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Yellow, new Position(5, 2), this), new ScreenPositon('f', 2).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Yellow, new Position(6, 2), this), new ScreenPositon('g', 2).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Yellow, new Position(7, 2), this), new ScreenPositon('h', 2).ToPosition(this.Board));

            //red
            PlacePiece(new Rook(this.Board, Color.Red), new ScreenPositon('a', 8).ToPosition(this.Board));
            PlacePiece(new Knight(this.Board, Color.Red), new ScreenPositon('b', 8).ToPosition(this.Board));
            PlacePiece(new Bishop(this.Board, Color.Red), new ScreenPositon('c', 8).ToPosition(this.Board));
            PlacePiece(new King(this.Board, Color.Red, this), new ScreenPositon('e', 8).ToPosition(this.Board));
            PlacePiece(new Queen(this.Board, Color.Red), new ScreenPositon('d', 8).ToPosition(this.Board));
            PlacePiece(new Bishop(this.Board, Color.Red), new ScreenPositon('f', 8).ToPosition(this.Board));
            PlacePiece(new Knight(this.Board, Color.Red), new ScreenPositon('g', 8).ToPosition(this.Board));
            PlacePiece(new Rook(this.Board, Color.Red), new ScreenPositon('h', 8).ToPosition(this.Board));


            PlacePiece(new Pawn(this.Board, Color.Red, new Position(0, 7), this), new ScreenPositon('a', 7).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Red, new Position(1, 7), this), new ScreenPositon('b', 7).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Red, new Position(2, 7), this), new ScreenPositon('c', 7).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Red, new Position(3, 7), this), new ScreenPositon('d', 7).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Red, new Position(4, 7), this), new ScreenPositon('e', 7).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Red, new Position(5, 7), this), new ScreenPositon('f', 7).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Red, new Position(6, 7), this), new ScreenPositon('g', 7).ToPosition(this.Board));
            PlacePiece(new Pawn(this.Board, Color.Red, new Position(7, 7), this), new ScreenPositon('h', 7).ToPosition(this.Board));
        }

        public HashSet<Piece> GetRedCapturedPieces()
        {
            return this.CapturedPieces.Where(x => x.Color == Color.Red).ToHashSet();
        }

        public HashSet<Piece> GetYellowCapturedPieces()
        {
            return this.CapturedPieces.Where(x => x.Color == Color.Yellow).ToHashSet();
        }

        public ChessMove VerifyCheck(Color color, Piece piece = null)
        {
            var enemyPieces = this.MatchPieces.Where(x => x.Color != color);
            var king = this.MatchPieces.Where(x => x is King && x.Color == color).FirstOrDefault();

            foreach (var enemy in enemyPieces)
            {
                var inCheck = enemy.PossibleMovements()[king.Position.Line, king.Position.Column];
                var kingMovements = king.PossibleMovements();

                if (inCheck)
                {
                    for (int i = 0; i < this.Board.Lines; i++)
                    {
                        for (int j = 0; j < this.Board.Columns; j++)
                        {
                            if (kingMovements[i, j] == true && enemy.PossibleMovements()[i, j] == false)
                            {
                                return ChessMove.Check;
                            }
                        }
                    }

                    return ChessMove.Checkmate;
                }
            }

            return ChessMove.None;
        }

        public void Finish()
        {
            this.IsFinished = true;
        }

        public void CastlingMove(Piece piece)
        {
            var rooks = this.MatchPieces.Where(x => x.Color == this.CurrentPlayer && x is Rook);

            if (piece.Position.Column == 2)
            {
                var rook = rooks.FirstOrDefault(x => x.Position.Column == 0);
                MovePiece(rook.Position, new Position(piece.Position.Line, piece.Position.Column + 1));
            }
            else
            {
                var rook = rooks.FirstOrDefault(x => x.Position.Column == 7);
                MovePiece(rook.Position, new Position(piece.Position.Line, piece.Position.Column - 1));
            }

            this.CastlingPosition = null;
        }

        public void IsEnPassantMove(Piece piece, Position destination)
        {
            var direction = piece.Color == Color.Red ? 1 : -1;

            if (this.EnPassant?.Position?.Line + direction == destination.Line && this.EnPassant?.Position?.Column == destination.Column)
            {
                Piece pieceTrapped = this.Board.RemovePiece(this.EnPassant.Position);
                this.CapturedPieces.Add(pieceTrapped);
                this.MatchPieces.Remove(pieceTrapped);
            }

            this.EnPassant = null;
            this.EnPassantAllowed = false;
        }

        public void VerifyStealmate(Position position)
        {
            var stealmate = true;

            if (this.Board.IsValidPosition(position))
            {
                var piece = this.Board.Piece(position);

                if (piece is King)
                {
                    var kingMovements = piece.PossibleMovements();

                    for (int i = 0; i < this.Board.Lines; i++)
                    {
                        for (int j = 0; j < this.Board.Columns; j++)
                        {
                            if (kingMovements[i, j] == true)
                            {
                                stealmate = false;
                                break;
                            }
                        }

                        if (stealmate == false)
                            break;
                    }

                    if (stealmate)
                    {
                        this.check = stealmate == false ? ChessMove.None : ChessMove.Stalemate;
                        this.Finish();
                    }
                }
            }
        }

        public void VerifyPromotion(Piece piece)
        {
            Console.Clear();
            Console.WriteLine("Congrats pawn promotion, choose a Piece to recover:");
            this.Board.Promotion = true;
            var possiblePieces = this.PossiblePieces.Where(x => x.Color == piece.Color);
            var pieces = possiblePieces.Select(x => x.Name).Distinct();

            foreach (var capturedPiece in pieces)
            {
                Console.ForegroundColor = (ConsoleColor)piece.Color;
                Console.Write(capturedPiece + " ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" ");
            }

            Console.WriteLine();
            var description = Console.ReadLine();
            var recoveredPiece = possiblePieces.FirstOrDefault(x => x.Name.ToLower() == description.ToLower());

            this.MatchPieces.Add(recoveredPiece);
            this.Board.InsertPiece(recoveredPiece, piece.Position);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AIScript
{
    class AI
    {
        //arrays of numerics that need to be looped through
        private static int[] diceRolls = { 1, 2, 3, 4, 5, 6, 7, 10 };
        private static int[] pieces = { 2, 3, 4, 5 };
        private static int[] swallowSpecial = { 3, 4, 7, 13 };

        //indicates possible starting positions for the swallow
        private bool[] launched;
        private int endGameWeight = 2;

        //default constructor
        public AI()
        {
            launched = new bool[5] { false, false, false, false, false };
        }

        //encapsulates a "move" object with piece moved and intended destination
        public class Move
        {
            public int piece;
            public int destination;

            public Move(int piece, int destination)
            {
                this.piece = piece;
                this.destination = destination;
            }
        };

        //a node of the minimax tree that will be evaluated to determine next move
        private class Node
        {
            public Move move; //the move that led us to this state
            public int score; //how good of a move this is
            public List<Node> children; //other possible states that could be spawned from here

            public Node(Move move)
            {
                this.move = move;
                score = 0;
                children = new List<Node>();
            }
        };

        //returns true if the piece in question can enter the board at destination
        //for all pieces but the swallow, their breakout and target destination are
        //the same.
        private bool CanBreakout(int piece, int destination)
        {
            switch (piece)
            {
                case 1: //swallow
                    //swallow re-entry special case
                    return (!launched[0] && destination == 4) || (launched[0] && (
                        destination == 3 || destination == 7 || destination == 13
                        ));
                case 2: //storm-bird
                    return destination == 5 || launched[1];
                case 3: //raven
                    return destination == 6 || launched[2];
                case 4: //rooster
                    return destination == 7 || launched[3];
                case 5: //eagle
                    return destination == 10 || launched[4];
            }
            return false;
        }

        //does the destination match a rosette on the board?
        private bool IsSafe(int position)
        {
            return position == 4 || position == 8 || position == 14 || position == 15;
        }

        //is there a white or black piece on the board at that destination?
        private bool OccupiedBy(int[] board, int destination, bool whiteTeam)
        {
            int offset = whiteTeam ? -1 : 4;
            for (int i = 1; i <= 5; ++i)
            {
                if (board[i + offset] == destination)
                {
                    return true;
                }
            }
            return false;
        }

        //is the position provided in the zone-of-attack for the board?
        private bool AtWar(int position)
        {
            return 4 < position && position < 13;
        }

        //can the piece provided move to the destination given the state in board?
        private bool CanMove(int[] board, int piece, int destination, bool whiteTurn)
        {
            int offset = whiteTurn ? -1 : 4;

            //out of play
            if (destination > 15)
            {
                return false;
            }

            //first move breakout
            if (board[piece + offset] == 0 && !CanBreakout(piece, destination))
            {
                return false;
            }

            //check possible occupations
            if (IsSafe(destination))
            {
                //destination is safe and occupied by the enemy
                if (OccupiedBy(board, destination, !whiteTurn) && AtWar(destination)) //fixed double occupation bug
                {
                    return false;
                }
            }
            else
            {
                //destination is not safe and is occupied by a friendly
                if (OccupiedBy(board, destination, whiteTurn))
                {
                    return false;
                }
            }

            return true;
        }

        //produces a new board state based on the old state and the movement of a piece
        private int[] MakeMove(int[] board, int piece, int destination, bool whiteTurn)
        {
            int[] newBoard = (int[])board.Clone();
            int offset = whiteTurn ? -1 : 4;
            int enemyOffset = whiteTurn ? 4 : -1;

            //knock off the AI's piece if attacked
            for (int i = 1; i <= 5; ++i)
            {
                if (newBoard[i + enemyOffset] == destination)
                {
                    newBoard[i + enemyOffset] = 0;
                }
            }

            newBoard[piece + offset] = destination;
            return newBoard;
        }

        //score is based on who has more pieces farther along the board
        //biased towards getting pieces to the finish
        private int ScoreBoard(int[] board, bool whiteFavor)
        {
            int whiteSum = 0;
            int blackSum = 0;
            for (int i = 0; i < 5; ++i)
            {
                whiteSum += board[i];
                if (board[i] == 15)
                {
                    whiteSum += endGameWeight;
                }
                blackSum += board[i + 5];
                if (board[i + 5] == 15)
                {
                    blackSum += endGameWeight;
                }
            }

            int score = whiteSum - blackSum;
            return whiteFavor ? score : -score;
        }

        //constructs a minimax tree node given the provided inputs
        private Node BuildTree(int[] board, int depth, bool whiteTurn, Move move)
        {
            Node n = new Node(move);

            //hit the bottom of our tree
            if (depth == 0)
            {
                n.score = ScoreBoard(board, whiteTurn);
                return n;
            }

            int offset = whiteTurn ? -1 : 4;

            //handle swallow special cases
            if (board[1 + offset] == 0)
            {
                foreach (int s in swallowSpecial)
                {
                    if (CanMove(board, 1, s, whiteTurn))
                    {
                        int[] nextBoard = MakeMove(board, 1, s, whiteTurn);
                        Move nextMove = new Move(1, s);
                        n.children.Add(BuildTree(nextBoard, depth - 1, !whiteTurn, nextMove));
                    }
                }
            }
            //roll all possible dice and piece combinations
            foreach (int d in diceRolls)
            {
                foreach (int p in pieces)
                {
                    if (CanMove(board, p, board[p + offset] + d, whiteTurn))
                    {
                        int[] nextBoard = MakeMove(board, p, board[p + offset] + d, whiteTurn);
                        Move nextMove = new Move(p, board[p + offset] + d);
                        n.children.Add(BuildTree(nextBoard, depth - 1, !whiteTurn, nextMove));
                    }
                }
            }

            return n;
        }

        //generates scores based on the minimax algorithm with alphabeta pruning
        private int EvaluateTree(Node node, int depth, int alpha, int beta, bool maximizing)
        {
            //hit the bottom of the tree
            //second condition is if no moves are possible
            if (depth == 0 || node.children.Count == 0)
            {
                return node.score;
            }

            if (maximizing)
            {
                node.score = EvaluateTree(node.children[0], depth - 1, alpha, beta, false);
                alpha = Math.Max(alpha, node.score);
                for (int i = 1; i < node.children.Count && beta > alpha; ++i)
                {
                    node.score = Math.Max(node.score, EvaluateTree(node.children[i], depth - 1, alpha, beta, false));
                    alpha = Math.Max(alpha, node.score);
                }
                return node.score;
            }

            node.score = EvaluateTree(node.children[0], depth - 1, alpha, beta, true);
            for (int i = 1; i < node.children.Count && beta > alpha; ++i)
            {
                node.score = Math.Min(node.score, EvaluateTree(node.children[i], depth - 1, alpha, beta, true));
                beta = Math.Min(beta, node.score);
            }
            return node.score;
        }
        private int EvaluateFull(Node node, int depth, bool maximizing)
        {
            if (depth == 0 || node.children.Count == 0)
            {
                return node.score;
            }

            if (maximizing)
            {
                node.score = EvaluateFull(node.children[0], depth - 1, false);
                for (int i = 1; i < node.children.Count; ++i)
                {
                    node.score = Math.Max(node.score, EvaluateFull(node.children[i], depth - 1, false));
                }
                return node.score;
            }

            node.score = EvaluateFull(node.children[0], depth - 1, true);
            for (int i = 1; i < node.children.Count; ++i)
            {
                node.score = Math.Min(node.score, EvaluateFull(node.children[i], depth - 1, true));
            }
            return node.score;
        }

        //find the Move object which provides the minimized risk.
        //target score is returned by EvaluateTree()
        private List<Move> BestMove(Node n, int score)
        {
            List<Move> lst = new List<Move>();
            for (int i = 0; i < n.children.Count; ++i)
            {
                if (n.children[i].score == score)
                {
                    lst.Add(n.children[i].move);
                }
            }
            if (lst.Count == 0)
            {
                lst.Add(new Move(0, 0));
            }
            return lst;
        }

        //generate the AI's next move based on the current board state
        //and the dice roll the ai made. It's a minimax algorithm
        //evaluated down depth layers
        public Move NextMove(int[] board, int roll, int depth) //roll = dice value given, depth=higher is smarter (1-inf)
        {
            Node root = new Node(new Move(0, 0));

            //if a modified roll is provided, considers an unmodified roll as well
            if (roll >= 5)
            {
                //convert total dice result to base number-dice result
                int tempRoll = (roll == 5 ? 1 : (roll == 6 ? 2 : (roll == 7 ? 3 : 4 )));

                //handle swallow special cases
                if (board[5] == 0 && tempRoll == 2)
                {
                    foreach (int s in swallowSpecial)
                    {
                        if (CanMove(board, 1, s, false))
                        {
                            int[] nextBoard = MakeMove(board, 1, s, false);
                            Move nextMove = new Move(1, s);
                            root.children.Add(BuildTree(nextBoard, depth - 1, true, nextMove));
                        }
                    }
                }
                else if (board[5] != 0)
                {
                    if (CanMove(board, 1, board[5] + tempRoll, false))
                    {
                        int[] nextBoard = MakeMove(board, 1, board[5] + tempRoll, false);
                        Move nextMove = new Move(1, board[5] + tempRoll);
                        root.children.Add(BuildTree(nextBoard, depth - 1, true, nextMove));
                    }
                }
                //generate all possible piece combinations given dice roll
                foreach (int p in pieces)
                {
                    if (CanMove(board, p, board[p + 4] + tempRoll, false))
                    {
                        int[] nextBoard = MakeMove(board, p, board[p + 4] + tempRoll, false);
                        Move nextMove = new Move(p, board[p + 4] + tempRoll);
                        root.children.Add(BuildTree(nextBoard, depth - 1, true, nextMove));
                    }
                }
            }

            //handle swallow special cases
            if (board[5] == 0 && roll == 2)
            {
                foreach (int s in swallowSpecial)
                {
                    if (CanMove(board, 1, s, false))
                    {
                        int[] nextBoard = MakeMove(board, 1, s, false);
                        Move nextMove = new Move(1, s);
                        root.children.Add(BuildTree(nextBoard, depth - 1, true, nextMove));
                    }
                }
            }
            else if (board[5] != 0)
            {
                if (CanMove(board, 1, board[5] + roll, false))
                {
                    int[] nextBoard = MakeMove(board, 1, board[5] + roll, false);
                    Move nextMove = new Move(1, board[5] + roll);
                    root.children.Add(BuildTree(nextBoard, depth - 1, true, nextMove));
                }
            }
            //generate all possible piece combinations given dice roll
            foreach (int p in pieces)
            {
                if (CanMove(board, p, board[p + 4] + roll, false))
                {
                    int[] nextBoard = MakeMove(board, p, board[p + 4] + roll, false);
                    Move nextMove = new Move(p, board[p + 4] + roll);
                    root.children.Add(BuildTree(nextBoard, depth - 1, true, nextMove));
                }
            }

            //evaluate the newly generated tree and get the best possible move
            //for the AI given the provided roll
            int bestScore = EvaluateTree(root, depth, -1000, 1000, true);
            //int bestScore = EvaluateFull(root, depth, true);
            List<Move> moves = BestMove(root, bestScore);
            Move finalMove = moves[UnityEngine.Random.Range(0, moves.Count)];

            //update found positions based on how far the new piece has moved
            if (finalMove.piece != 0)
            {
                launched[finalMove.piece - 1] = true;
            }

            return finalMove;
        }
    }

    class Helper
    {
        private static int[] launch = { 2, 5, 6, 7, 10 };

        private static bool IsBlocked(int[] board, int pos)
        {
            return board[0] == pos || board[1] == pos ||
                board[2] == pos || board[3] == pos || board[4] == pos;
        }
        private static bool IsSafe(int pos)
        {
            return pos == 4 || pos == 8 || pos == 14 || pos == 15;
        }
        private static bool GenericRoll(int[] board, int target)
        {
            if (target <= 15)
            {
                if (!IsBlocked(board, target) || IsSafe(target))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool NonSpecialCheck(int[] board, bool[] pieceMoved, int piece, int roll)
        {
            int target = board[piece] + roll;

            if ( (!pieceMoved[piece] || roll == launch[piece]) && GenericRoll(board, target))
            {
                return true;
            }
            return false;
        }

        public static bool CanPlayerMove(int[] board, bool[] pieceMoved, int roll)
        {
            //swallow
            if (board[0] == 0) //swallow at start
            {
                if (roll == launch[0]) //swallow only launches on 2
                {
                    if (!pieceMoved[0]) //swallow has launched logic
                    {
                        if (!IsBlocked(board, 3) || !IsBlocked(board, 7) || !IsBlocked(board, 13)) //space available
                        {
                            return true;
                        }
                    }
                    else //swallow first launch logic
                    {
                        return true; //4 is a rosette and always available
                    }
                }
            }
            else //swallow in play
            {
                if (GenericRoll(board, board[0] + roll))
                {
                    return true;
                }
            }

            //everyone else
            for(int p = 1; p < 5; ++p)
            {
                if(NonSpecialCheck(board, pieceMoved, p, roll))
                {
                    return true;
                }
            }

            //no move
            return false;
        }
    }
}

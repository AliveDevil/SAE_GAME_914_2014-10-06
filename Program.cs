//#define Dictionary

using System;
#if Dictionary
using System.Collections.Generic;
#endif

namespace Game_2014_10_06
{
	class Program
	{
		const int Width = 20, Height = 10;
		const char EmptyChar = (char)0;
		const char EntranceChar = (char)0x25B2;
		const char FullBlockChar = (char)0x2588;
		const char PointChar = (char)0x263C;
		const char ObstacleChar = (char)0x2592;
		const char PlayerChar = (char)0x263A;
		const char ExitChar = (char)0x00A4;

		enum Field : byte
		{
			Invalid = 0x80,
			None = 0x01,
			Entrance = 0x02,
			Wall = 0x04,
			Point = 0x08,
			Obstacle = 0x10,
			Exit = 0x20
		};

		static int playerXPos, playerYPos, score, steps;
		static bool isGameOver;
		static Field MovableFieldTypes = Field.None | Field.Entrance | Field.Point | Field.Obstacle | Field.Exit;
		static bool firstRun;

#if Dictionary
		static Dictionary<Field, char> output = new Dictionary<Field, char>()
		{
			{Field.None, EmptyChar},
			{Field.Entrance, EntranceChar},
			{Field.Wall, FullBlockChar},
			{Field.Point, PointChar},
			{Field.Obstacle, ObstacleChar},
			{Field.Exit, ExitChar}
		};
#endif

		static Field[,] map = new Field[Height, Width];
		static byte[,] rawMap = new byte[Height, Width]
		{
			{ 0x01, 0x01, 0x01, 0x10, 0x01, 0x01, 0x01, 0x01, 0x01, 0x04, 0x04, 0x01, 0x01, 0x01, 0x01, 0x01, 0x04, 0x20, 0x04, 0x01 },
			{ 0x01, 0x04, 0x01, 0x04, 0x04, 0x04, 0x04, 0x04, 0x01, 0x01, 0x04, 0x01, 0x04, 0x10, 0x04, 0x01, 0x04, 0x01, 0x04, 0x01 },
			{ 0x08, 0x04, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x04, 0x01, 0x04, 0x01, 0x04, 0x01, 0x04, 0x01, 0x04, 0x01, 0x01, 0x01 },
			{ 0x04, 0x04, 0x01, 0x01, 0x04, 0x04, 0x04, 0x01, 0x04, 0x01, 0x01, 0x01, 0x04, 0x01, 0x04, 0x01, 0x10, 0x04, 0x01, 0x01 },
			{ 0x01, 0x01, 0x01, 0x04, 0x01, 0x01, 0x04, 0x01, 0x01, 0x04, 0x04, 0x04, 0x01, 0x01, 0x04, 0x01, 0x01, 0x01, 0x04, 0x01 },
			{ 0x01, 0x04, 0x04, 0x04, 0x04, 0x01, 0x04, 0x04, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x04, 0x01, 0x04, 0x01, 0x04, 0x01 },
			{ 0x01, 0x04, 0x01, 0x01, 0x01, 0x01, 0x01, 0x04, 0x04, 0x04, 0x01, 0x04, 0x01, 0x04, 0x01, 0x01, 0x04, 0x01, 0x01, 0x01 },
			{ 0x01, 0x04, 0x01, 0x04, 0x04, 0x04, 0x01, 0x04, 0x01, 0x01, 0x01, 0x04, 0x08, 0x04, 0x01, 0x04, 0x01, 0x01, 0x04, 0x01 },
			{ 0x01, 0x04, 0x01, 0x04, 0x08, 0x04, 0x04, 0x04, 0x01, 0x04, 0x01, 0x04, 0x04, 0x01, 0x01, 0x04, 0x01, 0x04, 0x01, 0x01 },
			{ 0x02, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x04, 0x08, 0x04, 0x01, 0x01, 0x01, 0x01, 0x04, 0x01, 0x01, 0x04, 0x08, 0x04 }
		};

		static void Main(string[] args)
		{
			Console.CursorVisible = false;

			firstRun = true;
			WriteIntro();
			BuildMap();
			while (!isGameOver)
			{
				DoInput();
				PerformCheck();
				DrawMap();
			}
			Console.Clear();
			Console.SetCursorPosition(0, 0);
			Console.WriteLine("Dein Punktestand: {0}\nBenötigte Schritte: {1}", score, steps);
			Console.WriteLine("===");
			Console.WriteLine("Taste zum Beenden drücken.");
			Console.ReadKey();
		}

		static void WriteIntro()
		{
			Console.WriteLine("Willkommen zu einem weiteren Labyrinth im Konsolen-Stil.");
			Console.WriteLine("Gehe (mit WASD bzw. den Pfeiltasten) durch das Labyrinth und finde den Ausgang {0}.", ExitChar);
			Console.WriteLine("Auf deinem Weg findest du Punkte {0} (+50 Punkte im Score) und Hindernisse {1} (-10 Punkte im Score).", PointChar, ObstacleChar);
			Console.WriteLine("Wer hat die meisten Punkte mit den wenigsten Schritten?");
			Console.WriteLine();
			Console.WriteLine("Drücke eine beliebige Taste zum Starten.");
		}

#if !Dictionary
		static char ResolveType(Field type)
		{
			switch (type)
			{
				case Field.Entrance:
					return EntranceChar;
				case Field.Wall:
					return FullBlockChar;
				case Field.Point:
					return PointChar;
				case Field.Obstacle:
					return ObstacleChar;
				case Field.Exit:
					return ExitChar;
				default:
					return EmptyChar;
			}
		}
#endif

		static void BuildMap()
		{
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					map[y, x] = (Field)rawMap[y, x];
				}
			}
		}

		static void DoInput()
		{
			ConsoleKeyInfo key = Console.ReadKey(true);
			switch (key.Key)
			{
				case ConsoleKey.W:
				case ConsoleKey.UpArrow:
					if (CanWalk(0, 1))
					{
						playerYPos++;
						steps++;
					}
					break;
				case ConsoleKey.S:
				case ConsoleKey.DownArrow:
					if (CanWalk(0, -1))
					{
						playerYPos--;
						steps++;
					}
					break;
				case ConsoleKey.D:
				case ConsoleKey.RightArrow:
					if (CanWalk(1, 0))
					{
						playerXPos++;
						steps++;
					}
					break;
				case ConsoleKey.A:
				case ConsoleKey.LeftArrow:
					if (CanWalk(-1, 0))
					{
						playerXPos--;
						steps++;
					}
					break;
			}
		}
		static bool CanWalk(int dX, int dY)
		{
			int dXPos = playerXPos + dX;
			int dYPos = playerYPos + dY;

			if (MovableFieldTypes.HasFlag(FieldAtPosition(dXPos, dYPos)))
			{
				return true;
			}

			return false;
		}

		static void PerformCheck()
		{
			switch (FieldAtPosition(playerXPos, playerYPos))
			{
				case Field.Obstacle:
					score -= 10;
					ResetFieldAtPosition(playerXPos, playerYPos);
					break;
				case Field.Point:
					score += 50;
					ResetFieldAtPosition(playerXPos, playerYPos);
					break;
				case Field.Exit:
					isGameOver = true;
					break;
			}
		}

		static void DrawMap()
		{
			if (firstRun)
			{
				firstRun = false;
				Console.Clear();
			}
			DrawStats();
			DrawSurround();
			DrawEntities();
			DrawPlayer();
		}
		static void DrawStats()
		{
			for (int i = 0; i < Console.BufferWidth; i++)
			{
				Console.SetCursorPosition(i, 0);
				Console.Write('\0');
			}
			Console.SetCursorPosition(0, 0);
			Console.Write("Punkte: {0:0000} Schritte: {1:0000}", score, steps);
		}
		static void DrawSurround()
		{
			DrawTopBottom();
			DrawLeftRight();
		}
		static void DrawTopBottom()
		{
			for (int x = 0; x < Width + 2; x++)
			{
				DrawItem(x, 0, FullBlockChar);
				DrawItem(x, Height + 1, FullBlockChar);
			}
		}
		static void DrawLeftRight()
		{
			for (int y = 0; y < Height + 2; y++)
			{
				DrawItem(0, y, FullBlockChar);
				DrawItem(Width + 1, y, FullBlockChar);
			}
		}
		static void DrawEntities()
		{
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
#if Dictionary
					drawItem(x + 1, y + 1, output[map[y, x]]);
#else
					DrawItem(x + 1, y + 1, ResolveType(map[y, x]));
#endif
				}
			}
		}
		static void DrawPlayer()
		{
			DrawItem(playerXPos + 1, Height - playerYPos, PlayerChar);
		}

		static void DrawItem(int x, int y, char @char)
		{
			Console.SetCursorPosition(x, y + 3);
			Console.Write(@char);
		}
		static Field FieldAtPosition(int x, int y)
		{
			if (x < 0 | y < 0 | x >= Width | y >= Height)
			{
				return Field.Invalid;
			}

			return map[Height - (y + 1), x];
		}
		static void ResetFieldAtPosition(int x, int y)
		{
			if (x < 0 | y < 0 | x >= Width | y >= Height)
			{
				return;
			}

			map[Height - (y + 1), x] = Field.None;
		}
	}
}

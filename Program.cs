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

			Console.WriteLine("Press [any key] to start ..");
			buildMap();
			while (!isGameOver)
			{
				doInput();
				performCheck();
				drawMap();
			}
			Console.Clear();
			Console.SetCursorPosition(0, 0);
			Console.WriteLine("Dein Punktestand: {0}\nBenötigte Schritte: {1}", score, steps);
			Console.WriteLine("===");
			Console.WriteLine("Taste zum Beenden drücken.");
			Console.Read();
		}

#if !Dictionary
		static char resolveType(Field type)
		{
			switch(type)
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

		static void buildMap()
		{
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					map[y, x] = (Field)rawMap[y, x];
				}
			}
		}

		static void doInput()
		{
			ConsoleKeyInfo key = Console.ReadKey(true);
			switch (key.Key)
			{
				case ConsoleKey.W:
				case ConsoleKey.UpArrow:
					if (canWalk(0, 1))
					{
						playerYPos++;
						steps++;
					}
					break;
				case ConsoleKey.S:
				case ConsoleKey.DownArrow:
					if (canWalk(0, -1))
					{
						playerYPos--;
						steps++;
					}
					break;
				case ConsoleKey.D:
				case ConsoleKey.RightArrow:
					if (canWalk(1, 0))
					{
						playerXPos++;
						steps++;
					}
					break;
				case ConsoleKey.A:
				case ConsoleKey.LeftArrow:
					if (canWalk(-1, 0))
					{
						playerXPos--;
						steps++;
					}
					break;
			}
		}
		static bool canWalk(int dX, int dY)
		{
			int dXPos = playerXPos + dX;
			int dYPos = playerYPos + dY;

			if (MovableFieldTypes.HasFlag(fieldAtPosition(dXPos, dYPos)))
			{
				return true;
			}

			return false;
		}

		static void performCheck()
		{
			switch (fieldAtPosition(playerXPos, playerYPos))
			{
				case Field.Obstacle:
					score -= 10;
					break;
				case Field.Point:
					score += 50;
					break;
				case Field.Exit:
					isGameOver = true;
					break;
			}
		}

		static void drawMap()
		{
			drawStats();
			drawSurround();
			drawEntities();
			drawPlayer();
		}
		static void drawStats()
		{
			for (int i = 0; i < Console.BufferWidth; i++)
			{
				Console.SetCursorPosition(i, 0);
				Console.Write('\0');
			}
			Console.SetCursorPosition(0, 0);
			Console.WriteLine("Punkte: {0:0000} Schritte: {1:0000}", score, steps);
		}
		static void drawSurround()
		{
			drawTopBottom();
			drawLeftRight();
		}
		static void drawTopBottom()
		{
			for (int x = 0; x < Width + 2; x++)
			{
				drawItem(x, 0, FullBlockChar);
				drawItem(x, Height + 1, FullBlockChar);
			}
		}
		static void drawLeftRight()
		{
			for (int y = 0; y < Height + 2; y++)
			{
				drawItem(0, y, FullBlockChar);
				drawItem(Width + 1, y, FullBlockChar);
			}
		}
		static void drawEntities()
		{
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
#if Dictionary
					drawItem(x + 1, y + 1, output[map[y, x]]);
#else
					drawItem(x + 1, y + 1, resolveType(map[y, x]));
#endif
				}
			}
		}
		static void drawPlayer()
		{
			drawItem(playerXPos + 1, Height - playerYPos, PlayerChar);
		}

		static void drawItem(int x, int y, char @char)
		{
			Console.SetCursorPosition(x, y + 3);
			Console.Write(@char);
		}
		static Field fieldAtPosition(int x, int y)
		{
			if (x < 0 | y < 0 | x >= Width | y >= Height)
			{
				return Field.Invalid;
			}

			return map[Height - (y + 1), x];
		}
	}
}

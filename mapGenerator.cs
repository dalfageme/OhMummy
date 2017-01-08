//David Alfageme

/*TRANSFORM TO ARRAY VERSION
 * 
 * map
 * Value for default black = 0
 * Value for black EXIT    = 6
 * Value of yellow map     = 1
 * Value of sarcophagus    = 2
 * Value of opened sarc.   = 3 
 * Special sarc.           = 5
 * Value of trail          = 4
 * 
 * 
 * */
using System;
public class mapGenerator
{
    static byte[,] map = new byte[24,80];
    public static void Main()
    {
        byte xPosition = 10;
        byte yPosition = 4;
        byte xLeftLimit = 6;
        byte yTopLimit = 4;
        byte yBottomLimit = (byte)(map.GetLength(0) - 4);
        byte xRightLimit = (byte)(map.GetLength(1) - xLeftLimit -2);
        string fileName = Console.ReadLine();
        for (int row=0; row<24; row++)
        {
            for (int col=0; col<79;col++)
            {
                if ( row <  yTopLimit || row > yBottomLimit ||  col < xLeftLimit || col > xRightLimit)
                {
                    map[row,col] = 1;
                    Console.SetCursorPosition(col,row);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.Write(" ");
                }
            }
            Console.WriteLine();
        }
        
        Console.ResetColor();
        byte pintar = 0;
        ConsoleKeyInfo tecla;
        do
        {
            tecla =  Console.ReadKey();
            switch (tecla.Key)
            {
                case ConsoleKey.RightArrow:
                    if (xPosition < xRightLimit)
                    {
                        xPosition++;
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (xPosition > xLeftLimit)
                    {
                        xPosition--;
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (yPosition > yTopLimit)
                    {
                        yPosition--;
                    }

                    break;
                case ConsoleKey.DownArrow:
                    if (yPosition < yBottomLimit)
                    {
                        yPosition++;
                    }
                    break;
                    
                case ConsoleKey.Z:
                    pintar = 1;
                    break;
                case ConsoleKey.X:
                    pintar = 2;
                    break;
                case ConsoleKey.C:
                    pintar = 3;
                    break;
                case ConsoleKey.E:
                    while (!Console.KeyAvailable);
                    break;
                
            }
            
            Console.SetCursorPosition(xPosition,yPosition);
            switch ( pintar)
            {
                case 1:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    map[yPosition,xPosition] = 1;
                    break;
                    
                case 2: 
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.BackgroundColor = ConsoleColor.Gray;
                    map[yPosition,xPosition] = 2;
                    break;
                case 3: 
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Black;
                    map[yPosition,xPosition] = 0;
                    break;
            }
            
        }while(tecla.Key != ConsoleKey.Escape);
        Console.WriteLine("GENERATING FILE");
        Console.ResetColor();
        System.IO.StreamWriter file = new System.IO.StreamWriter(@fileName + ".map");
        for ( int row = 0; row < 24; row++)
        {
            for ( int col = 0; col < 80; col++ )
                file.Write(Convert.ToString(map[row,col]));
            file.Write(".");
        }
        file.Close();
    }
    
    public static void generateFile()
    {
        
    }
}

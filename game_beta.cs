/*ARRAY INFO
 * 
 * map
 * Value for default black = 0
 * Value of yellow map     = 1
 * Value of sarcophagus    = 2
 * Value of opened sarc.   = 3 
 * key sarc.               = 5
 * tomb sarc.              = 6
 * Value of trail          = 4
 * 
 * */
/*
 * OhMummy console game. 
 * The player should sourrond the sarcophagous to open it 
 * running away from the mummies. To go next level the player need
 * to open the tomb and get the key. On the next level one mummie more is waiting.
 * 
 * */
//David Alfageme
using System;
using System.IO;
public class MyGame
{
    static byte sarcIndex = 9;
    static byte sarcTomb = 0, sarcKey = 0, sarcmummy;
    const byte xLeftLimit = 6, yTopLimit = 4;
    static byte[,] map = new byte[24,80];
    static Random generator = new Random();
    static byte yBottomLimit = (byte)(map.GetLength(0) - 4);
    static byte xRightLimit = (byte)(map.GetLength(1) - xLeftLimit -2);
    static int characterCol=39, characterRow=4;
    static bool easy = true;
    static int score=000;
    static bool loser = false;
    static int startLives = 3, lives=startLives;
    static byte mummyDelay = 0;
    static bool keyopen = false, tombopen = false;
    static byte level = 0;
    static byte[,] mapSarc = new byte[24,80];
    public struct mummy
    {
        public int x;
        public int y;
        public bool available;
    }
    
    static mummy[] mummys = new mummy[2];
    public static void Main()
    {
        byte [,]oldMap = map;
        gameModeSelection();
        map = LoadMap();
        mapSarc = getMapSarc(map);
        makeFile( mapSarc, "sarcMapCreated.txt");
        printConsole(map);

        for (int i = 0;  i < mummys.Length; i++)
        {
            mummyrandomPosition(i);
        }

        paintChamp(characterCol, characterRow);
        while ( !loser )
        {
            paintChanges ( map, oldMap );
            Array.Copy(map, 0, oldMap, 0, map.Length); 
            displayScore(score);
            displayLives(lives, startLives);
            bool mapChange = false; 
            while (!mapChange)
            {
                mummyMove();
                enemyCollision(ref mapChange);
                moveChar ();
                sarcSourronded (ref map, ref mapChange, ref mapSarc);
                System.Threading.Thread.Sleep(50);
            }
            nextLevel();
        }
        YouLose();
    }
    public static void YouLose()
    {
        Console.Clear();
        string endmsg = "YOU LOSE";
        Console.SetCursorPosition( 40 - endmsg.Length/2, 12 );
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(endmsg);
        string txtScore = "SCORE: ";
        Console.SetCursorPosition( 38 - txtScore.Length/2, 13);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(txtScore + score.ToString("0000")); 
        Console.ReadLine();
    }
    public static byte[,] LoadMap ()
    {
        //LOAD MAP FROM ARCHIVE
        byte[,] map = new byte[24,80];
        string file;
        try
        {
            DirectoryInfo di = new DirectoryInfo( Directory.GetCurrentDirectory() );
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() , "*.map");
            if ( level >= files.GetLength(0) )
                file = File.ReadAllText( files[files.GetLength(0)-1] );
            else
                file = File.ReadAllText( files[level] );
        }
        catch 
        {
            return defMap();
        }
        
        string[] rowsText = file.Split('.');
        for ( int row = 0; row < 24; row++)
        {
            string rowText = rowsText[row];
            for ( int col = 0; col < 80; col++ )
            {
                map[row,col] = Convert.ToByte(rowText.Substring(col,1));
            }
        }
        return map;
    }
    
    public static byte[,] defMap ()
    {
        byte[,] map = new byte[24,80];
        Console.BackgroundColor = ConsoleColor.Yellow;
        for (int row=0; row<24; row++)
        {
            for (int col=0; col<79;col++)
            {
                if ( row <  4 || row > 20  ||  col < 16 || col > 61)
                {
                    map[row, col] = 1; 
                }
                else
                {
                    map[row, col] = 0; 
                }
            }
            Console.WriteLine();
        }
        
        //ADD SARC TO MAP
        for (int row=0; row<=15; row++)
        {
            for (int col=0; col<45;col++)
            {
                if (col%9!=0 && row%4!=0)
                {
                    map [ row+4, col+16] = 2;
                }
            }
        }
        
        return map;
    }
    
    public static bool gameModeSelection ()
    {
        bool change = false; 
        ConsoleKeyInfo selection;
        Console.SetCursorPosition(31, 11);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("CHOOSE GAME MODE");
        Console.SetCursorPosition(37, 12);
        Console.BackgroundColor = ConsoleColor.Green;
        Console.Write(" EASY ");
        Console.BackgroundColor = ConsoleColor.Black;
        selection = Console.ReadKey();
        
        //GAME MODE SELECTION
        while (selection.Key != ConsoleKey.Enter)
        {
            Console.Clear();
            Console.SetCursorPosition(31, 11);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("CHOOSE GAME MODE");
            Console.SetCursorPosition(37, 12);
            Console.ForegroundColor = ConsoleColor.Black;
            if (change)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write(" EASY ");
                Console.BackgroundColor = ConsoleColor.Black;
                easy = true;
                change = false;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write(" HARD ");
                Console.BackgroundColor = ConsoleColor.Black;
                easy = false;
                change = true;
            }
            selection = Console.ReadKey();
        }
        return easy;
    }
    
    public static byte[,] getMapSarc ( byte[,] map )
    {
        byte[,] mapSarc = new byte [24,80];
        for ( int row = 0; row < 24; row++)
        {
            for ( int col = 0; col < 80; col++)
            {
                if ( map [row, col] == 2)
                {
                    mapSarc[row, col] = 2;
                }
            }
        }
        
        for ( int row = yTopLimit; row < yBottomLimit + 1; row++)
        {
            for ( int col = xLeftLimit; col < xRightLimit + 1; col++)
            {
                byte sarcnumber = sarcIndex;
                bool belongsToAnother = false;
                if (mapSarc[ row , col] == 2 )
                {
                    //look top and bottom of each sarc
                    for ( int i = -1; i <= 1; i++)
                    {
                        if (mapSarc[ row - 1, col + i] >= 9 )
                        {
                            sarcnumber = mapSarc[ row - 1, col + i ];
                            belongsToAnother = true;
                        }
                        if (mapSarc[ row + 1, col + i] >= 9 )
                        {
                            sarcnumber = mapSarc[ row + 1, col + i ];
                            belongsToAnother = true;
                        }
                    }
                    //lool left
                    if (mapSarc[ row , col - 1] >= 9 )
                    {
                        sarcnumber = mapSarc[ row , col - 1 ];
                        belongsToAnother = true;
                    }
                    
                    //look right
                    if (mapSarc[ row , col - 1] >= 9 )
                    {
                        sarcnumber = mapSarc[ row , col - 1 ];
                        belongsToAnother = true;
                    }
                    
                    if (belongsToAnother)
                    {
                        mapSarc[ row , col ] = sarcnumber;
                    }
                    else
                    {
                        mapSarc[ row , col ] = ++sarcIndex;
                    }
                }
            }
        }
        sarcKey = Convert.ToByte(generator.Next(0, sarcIndex-10));
        sarcmummy = Convert.ToByte(generator.Next(0, sarcIndex-10));
        do 
        {
            sarcTomb = Convert.ToByte( generator.Next(0, sarcIndex-10));
        }
        while(sarcKey == sarcTomb);
        return mapSarc;
    }
    
    public static void makeFile (byte[,] map, string fileName)
    {
        //Make a file to saw possible errors
        System.IO.StreamWriter file2 = new System.IO.StreamWriter(@fileName);
        for ( int row = 0; row < map.GetLength(0); row++)
        {
            for ( int col = 0; col < map.GetLength(1); col++ )
                file2.Write(Convert.ToString(map[row,col]));
            file2.WriteLine(".");
        }
        file2.Close();
    }
    
    public static void printConsole ( byte[,] map)
    {
         //PAINT CONSOLE
        for (int row = 0 ; row < map.GetLength(0); row++)
        {
            for (int col = 0 ; col < map.GetLength(1); col++)
            {
                switch ( map[ row, col] )
                {
                    case 0:
                        Console.BackgroundColor = ConsoleColor.Black;
                        break;
                    case 1:
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        break;
                    case 2:
                        Console.BackgroundColor = ConsoleColor.Gray;
                        break;
                } 
                Console.SetCursorPosition(col,row);
                Console.Write(" ");   
            }      
        }
    }
    
    
    public static void paintChanges ( byte[,] map, byte[,] oldMap)
    {
        Console.SetCursorPosition(0,0);
        for (int row = 0 ; row < map.GetLength(0); row++)
        {
            for (int col = 0 ; col < map.GetLength(1); col++)
            {
                if (map[ row, col] != oldMap[ row, col])
                {
                    Console.SetCursorPosition(col,row);
                    switch ( map[ row, col] )
                    {
                        case 3:
                            Console.BackgroundColor = ConsoleColor.DarkMagenta;
                            Console.Write(" ");
                            break;
                        case 5:
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.Write(" ");
                            break;
                        case 6: 
                            Console.BackgroundColor = ConsoleColor.Cyan;
                            Console.Write(" ");
                            break;
                    }
                    
                }
            }    
        }
    }
    
    public static void displayScore (int score)
    {
        Console.BackgroundColor = ConsoleColor.Yellow;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.SetCursorPosition(16, 2);
        Console.Write("SCORE: ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(String.Format("{0:0000}", score));
    }
    
    public static void displayLives ( int lives, int startLives)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.SetCursorPosition(50, 2);
        Console.Write("LIVES:");
        Console.ForegroundColor = ConsoleColor.Blue;
        
        for (int i = 0; i < lives; i++)
        {
            Console.Write(" "+(char)3);
        }
        for (int i = lives; i < startLives; i++)
        {
            Console.Write("  ");
        }
    }
    
    public static byte enemySpeed (int score)
    {
        if (score < 200)
        {
            return 8;
        }
        else if ( score < 2000 )
        {
            return 8;
        }
        else if ( score < 5000 ) 
        {
            return  8;
        }
        else if ( score < 15000 )
        {
            return  8;
        }
        else
        {
            return 8;
        }
    }
    
    public static bool canMove ( int row, int col)
    {
        if ( row >= 24 || col >= 80 || col < 0 || row < 0 )
            return false;
        
        return map[row, col] == 4 || map[row, col] == 0;
    }
    
    public static void moveChar ()
    {
        if (Console.KeyAvailable)
        {
            do
            {
                Console.SetCursorPosition(0,24);
                ConsoleKeyInfo tecla =  Console.ReadKey();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.SetCursorPosition(characterCol,characterRow);
                if ( map[characterRow,characterCol] != 6)
                {
                    map[characterRow,characterCol] = 4;
                    Console.Write("·");
                }
                else
                {
                    Console.Write(" ");
                }
                
                switch (tecla.Key)
                {
                    case ConsoleKey.RightArrow:
                        if ( canMove(characterRow, characterCol+1) )
                            characterCol++;
                        break;
                        
                    case ConsoleKey.LeftArrow:
                        if ( canMove(characterRow, characterCol-1) )
                            characterCol--;
                        break;
                        
                    case ConsoleKey.UpArrow:
                        if ( canMove(characterRow-1, characterCol) )
                            characterRow--;
                        break;
                        
                    case ConsoleKey.DownArrow:
                    
                        if ( canMove(characterRow+1, characterCol) )
                            characterRow++;
                        break;
                        
                    case ConsoleKey.E:
                        while (!Console.KeyAvailable);
                        break;
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.SetCursorPosition(characterCol,characterRow);
                Console.Write(""+ (char)2);
                
            }
            while (Console.KeyAvailable);
        }
    }
    
    public static void sarcSourronded ( ref byte[,]map, ref bool mapChange, ref byte[,] mapSarc)
    {
        bool [] openedSarc = new bool [sarcIndex -10 + 1];
        for ( int i = 0; i < openedSarc.Length ; i++)
        {
            openedSarc[i] = true;
        }
        
        for ( int row = yTopLimit; row < yBottomLimit + 1; row++)
        {  
            for ( int col = xLeftLimit; col < xRightLimit + 1; col++)
            {
                if (mapSarc[ row , col] >= 10 )
                {   
                    bool issourronded = true;
                    
                    //left top
                    if ( map [row -1, col -1 ] == 0)
                    {
                        issourronded = false;
                    }
                    
                    //top
                    if ( map [row-1, col ] == 0)
                    {
                        issourronded = false;
                    }
                    //right top
                    if ( map [row-1, col +1 ] == 0)
                    {
                        issourronded = false;
                    }
                    //left
                    if ( map [row, col -1 ] == 0)
                    {
                        issourronded = false;
                    }
                    //right
                    if ( map [row, col + 1 ] == 0)
                    {
                        issourronded = false;
                    }
                    //bottom left
                    if ( map [row+1, col -1 ] == 0)
                    {
                        issourronded = false;
                    }
                    //bottom
                    if ( map [row+1, col ] == 0)
                    {
                        issourronded = false;
                    }
                    //bottom right
                    if ( map [row+1, col +1 ] == 0)
                    {
                        issourronded = false;
                    }
                    
                    if (!issourronded)
                    {
                        openedSarc[ mapSarc[row, col] -10] = false;
                    }
                }
            }
        }
        for ( int i = 0; i < openedSarc.Length; i++)
        {
            if( openedSarc[i] )
            {   
                for ( int row = yTopLimit; row <  yBottomLimit +1; row++)
                {
                    for ( int col = xLeftLimit; col < xRightLimit + 1; col++)
                    {
                        if (mapSarc [row, col] == i+10 && map[row, col] != 3
                            && map[row, col] != 5 && map[row, col] != 6)
                        {
                            if ( i == sarcKey )
                            {
                                map[row, col] = 5;
                                keyopen = true;
                            }
                            else if ( i == sarcTomb ) 
                            {
                                map[row, col] = 6;
                                tombopen = true;
                            }
                            else
                            {
                                map[row, col] = 3;
                            }
                            mapChange = true;
                            score+=10;
                        }
                    }
                }
            }
        }
    }
    public static void mummyMove()
    {
        mummyDelay++; //Delay for easy mummy
        for ( int i = 0; i < mummys.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition( mummys[i].x,mummys[i].y ); 
            byte mummySpeed = enemySpeed(score);
            if (mummyDelay%mummySpeed == 0)
            {
                mummyDelay = 0;
                if (map[mummys[i].y, mummys[i].x] == 4 && easy)
                {
                    Console.Write("·");
                }
                else 
                {
                    Console.Write(" ");
                    map[mummys[i].y, mummys[i].x] = 0;
                }
                
                
                if (mummys[i].x > characterCol)
                {
                    if (map[mummys[i].y, mummys[i].x-1] == 4 || 
                        map[mummys[i].y, mummys[i].x-1] == 0)
                    {
                        mummys[i].x--;
                    }
                    
                }
                else if (mummys[i].x < characterCol)
                {
                    if (map[mummys[i].y, mummys[i].x+1] == 4 || 
                        map[mummys[i].y, mummys[i].x+1] == 0) 
                    {
                        mummys[i].x++;  
                    }
                }
               
                if (mummys[i].y > characterRow)
                {
                    if (map[mummys[i].y-1, mummys[i].x] == 4 || 
                        map[mummys[i].y-1, mummys[i].x] == 0 )
                    {
                        mummys[i].y--;  
                    }
                    
                }
                else if (mummys[i].y < characterRow) 
                {
                    if (map[mummys[i].y+1, mummys[i].x] == 4 || 
                        map[mummys[i].y+1, mummys[i].x] == 0)
                    {
                        mummys[i].y++;  
                    }
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.SetCursorPosition(mummys[i].x,mummys[i].y);
                Console.Write(""+ (char)5);
            }
        }
        mummysAtSamePosition();
    }
    
    public static void paintChamp (int characterCol,int characterRow)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.SetCursorPosition(characterCol,characterRow);
        Console.Write(""+ (char)2);
    }
    
    public static void enemyCollision(ref bool mapChange)
    {
        for ( int i = 0; i < mummys.Length; i++)
        {
            if (mummys[i].x == characterCol && mummys[i].y == characterRow)
            {
                lives--;
                if (lives <= 0)
                {
                    loser = true;
                }
                mummyrandomPosition(i);
                paintChamp(characterCol, characterRow);
                mapChange = true;
            } 
        }
    }
    
    public static void mummyrandomPosition ( int index )
    {
        do
        {
            mummys[index].x = generator.Next(xLeftLimit, xRightLimit);
            mummys[index].y = generator.Next(yTopLimit, yBottomLimit);
        }
        while( !canMove(mummys[index].y, mummys[index].x) );
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.SetCursorPosition(mummys[index].x,mummys[index].y);
        Console.Write(""+ (char)5);
        
    }
    
    public static void nextLevel ()
    {
        if ( keyopen && tombopen )
        {
            level++;
            sarcIndex = 9;
            Console.WriteLine("NEXT LEVEL");
            map = LoadMap();
            mapSarc = getMapSarc(map);
            printConsole(map);
            makeFile( mapSarc, "segundomapsarc.txt");
            Array.Resize(ref mummys, mummys.Length + 1);
            mummyrandomPosition(mummys.Length-1);
            keyopen = false; tombopen = false;
            sarcKey = Convert.ToByte(generator.Next(0, sarcIndex-10));
            sarcmummy = Convert.ToByte(generator.Next(0, sarcIndex-10));
            do 
            {
                sarcTomb = Convert.ToByte( generator.Next(0, sarcIndex-10));
            }
            while(sarcKey == sarcTomb);
            
        }
    }
    public static void mummysAtSamePosition()
    {
        for ( int i = 0; i < mummys.Length-1; i++)
        {
            if( mummys[i].x == mummys[i+1].x  && mummys[i].y == mummys[i+1].y)
            {
                mummyrandomPosition(i);
                mummyrandomPosition(i+1);
            }
        }
    }
}


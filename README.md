# WordMasterMind
[![.NET](https://github.com/FreddieMercurial/WordMasterMind/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/FreddieMercurial/WordMasterMind/actions/workflows/dotnet.yml)
- A C# Blazor WebAssembly quick wordle clone based on the scrabble dictionary
- Self-contained

-----
Welcome to **Word MasterMind**!

Word Mastermind is a variation of [Mastermind](https://en.wikipedia.org/wiki/Mastermind_(board_game)), popularized by the game Wordle, but with a few differences.

In Word Mastermind, you can choose any word length in the [Scrabble](https://en.wikipedia.org/wiki/Scrabble) [Dictionary](https://scrabble.merriam.com/)

Words available in the dictionary range from 2 letters to 15 letters. Playing a 2 letter game is probably too easy, but there seemed no reason to limit things.

Unlike Wordle, the number of tries varies with the word length.

*   One (1) Attempt per letter in the secret word
*   One (1) additional Attempt normally, or Two (2) additional Attempts if you've selected **Hard Mode**

Therefore, a 3 letter word will give you 4 attempts, a 5 letter will give you 6 like Wordle, and a bonus try for Hard Mode. A 15 letter word would give you 16 attempts.

Guess the word the Mastermind has in mind in within the number of tries.

Each guess must be a valid word of the correct length. Hit the enter button to submit.

After each guess, the color of the tiles will change to show how close your guess was to the word.

Scoring:

*   Two (2) points per attempt under maximum
*   Three (3) additional points each first time a letter is used, it is in the correct position
*   One (1) additional point for each new letter, but out of place
*   One (1) more point for a previously guessed letter once it is in the correct position

Scoring subject to change, at least until it is dialed in.

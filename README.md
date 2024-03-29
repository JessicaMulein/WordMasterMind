# WordMasterMind
- A C# Blazor WebAssembly quick wordle clone based on the scrabble dictionary
- Self-contained
- Testing and code coverage:
  - [![.NET](https://github.com/WordMasterMind/WordMasterMind/actions/workflows/dotnet.yml/badge.svg)](https://github.com/WordMasterMind/WordMasterMind/actions/workflows/dotnet.yml)
  - [![CodeQL](https://github.com/WordMasterMind/WordMasterMind/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/WordMasterMind/WordMasterMind/actions/workflows/codeql-analysis.yml)
  - Code Coverage: https://wordmastermind.github.io/WordMasterMind/


# About
See: the [About](https://github.com/WordMasterMind/WordMasterMind/wiki/About) page in the Wiki.

![Rules Screenshot](https://raw.githubusercontent.com/FreddieMercurial/WordMasterMind/main/WordMasterMindRules.png)
![Dictionaries Screenshot](https://raw.githubusercontent.com/FreddieMercurial/WordMasterMind/main/WordMasterMindDictionaries.png)
![Word Lenghts Screenshot](https://raw.githubusercontent.com/FreddieMercurial/WordMasterMind/main/WordMasterMindWordLengths.png)
-----
Welcome to **Word MasterMind**!

Word MasterMind is a variation of [Mastermind](https://en.wikipedia.org/wiki/Mastermind_(board_game)), popularized by the game Wordle, but with a few differences.

In Word MasterMind, you can guess and the game will choose a random word length in either the [Scrabble](https://en.wikipedia.org/wiki/Scrabble) [Dictionary](https://scrabble.merriam.com/), one of a couple other provided word lists, or we've given you the tools to use your own.

Words available in the Scrabble dictionary range from 2 letters to 15 letters.
- Detailed information on the other dictionaries is forthcoming

Unlike Wordle, the number of tries varies with the word length.

*   One (1) Attempt per letter in the secret word
*   One (1) additional Attempt

[This will probably change](https://github.com/WordMasterMind/WordMasterMind/discussions/2#discussioncomment-2144488)

Therefore, a 3 letter word will give you 4 attempts, a 5 letter will give you 6 like Wordle. A 15 letter word would give you 16 attempts.

Guess the word the Mastermind has in mind in within the number of tries.

Each guess must be a valid word of the correct length. Hit the enter button to submit.

After each guess, the color of the tiles will change to show how close your guess was to the word.

Scoring (idea):

*   Two (2) points per attempt under maximum
*   Three (3) additional points each first time a letter is used, it is in the correct position
*   One (1) additional point for each new letter, but out of place
*   One (1) more point for a previously guessed letter once it is in the correct position

Scoring subject to change, at least until it is dialed in.

# Documentation
Documentation is located in the toplevel repo's [Wiki](https://github.com/WordMasterMind/WordMasterMind/wiki)
- Documentation for the utility to add your own dictionaries is covered on the [Dictionary File Formats](https://github.com/WordMasterMind/WordMasterMind/wiki/Dictionary-File-Formats) page.

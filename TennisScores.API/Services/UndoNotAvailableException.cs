namespace TennisScores.API.Services;

public sealed class UndoNotAvailableException(string message)
    : InvalidOperationException(message);

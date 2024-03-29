﻿namespace TemporalAirlinesConcept.Common.Helpers;

public class Saga
{
    private List<string> _log;
    private Stack<Func<Task>> _compensations;
    private Func<List<string>, Task> _onCompensationError;
    private Func<List<string>, Task> _onCompensationComplete;

    public Saga(List<string> log)
    {
        _log = log;
        _compensations = new Stack<Func<Task>>();
    }

    public void OnCompensationError(Func<List<string>, Task> onCompensationError)
    {
        _onCompensationError = onCompensationError;
    }

    public void OnCompensationComplete(Func<List<string>, Task> onCompensationComplete)
    {
        _onCompensationComplete = onCompensationComplete;
    }

    public void AddCompensation(Func<Task> compensation)
    {
        _compensations.Push(compensation);
    }

    public async Task Compensate()
    {
        var i = 0;

        while (_compensations.Count > 0)
        {
            i++;
            var c = _compensations.Pop();

            try
            {
                _log.Add($"Attempting compensation {i}...");
                await c.Invoke();
                _log.Add($"Compensation {i} successful!");
            }
            catch
            {
                /* log details of all other compensations that have not yet been made if this is a show-stopper */
                await _onCompensationError(_log);
                return;
            }
        }

        await _onCompensationComplete(_log);
    }
}
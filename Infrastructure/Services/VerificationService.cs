using Application.Auth;
using Application.Interfaces.External;
using System.Collections.Concurrent;

namespace Infrastructure.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly ConcurrentDictionary<Guid, VerificationCode> _codes = [];
        private readonly ConcurrentDictionary<Guid, DateTime> _lastRequest = [];
        private readonly ConcurrentDictionary<Guid, object> _userLocks = [];

        private readonly TimeSpan _cooldown = TimeSpan.FromMinutes(1);

        public Task<bool> CanRequestNewCodeAsync(Guid userId)
        {
            if (!_codes.TryGetValue(userId, out var code))
                return Task.FromResult(true);

            if (code.IsExpired() || code.IsUsed)
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        public Task<string> GenerateCodeAsync(Guid userId, int expiryMinutes = 15)
        {
            if (_lastRequest.TryGetValue(userId, out var last) &&
                DateTime.UtcNow < last.Add(_cooldown))
            {
                var remaining = (last.Add(_cooldown) - DateTime.UtcNow).TotalMinutes;
                throw new InvalidOperationException($"You must wait {Math.Ceiling(remaining)} more minute(s) before requesting a new code.");
            }

            var code = VerificationCode.Create(userId, expiryMinutes);

            _codes[userId] = code;
            _lastRequest[userId] = DateTime.UtcNow;
            //Console.WriteLine(_codes.Count);
            return Task.FromResult(code.Code);
        }

        public Task<bool> ValidateCodeAsync(Guid userId, string code)
        {
            //Console.WriteLine(_codes.Count);

            if (!_codes.TryGetValue(userId, out var storedCode))
                return Task.FromResult(false);

            var lockObj = _userLocks.GetOrAdd(userId, _ => new object());

            lock (lockObj)
            {
                if (storedCode.IsExpired() || storedCode.IsUsed || storedCode.Code != code)
                {
                    _codes.TryRemove(userId, out _);
                    return Task.FromResult(false);
                }

                storedCode.MarkAsUsed();
                _codes.TryRemove(userId, out _);
                _userLocks.TryRemove(userId, out _);
                return Task.FromResult(true);
            }
        }
    }
}
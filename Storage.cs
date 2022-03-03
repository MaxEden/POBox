using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;

namespace POBox
{
    public class Storage<T> : IStorage
    {
        private readonly ConcurrentDictionary<int, ParcelT> _parcels = new ConcurrentDictionary<int, ParcelT>();

        private int _uk;

        public int Post(T data, TimeSpan lifeTime, Action<T> disposer = null, string from = "", string password = "")
        {
            int id = GetId();

            var parcel = new ParcelT()
            {
                Id = id,
                From = from,
                Password = password,
                ExpirationDate = DateTimeOffset.Now + lifeTime,
                Data = data,
                Disposer = disposer
            };


            int token = GetToken(id);
            _parcels.TryAdd(id, parcel);
            return token;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private int GetId()
        {
            _uk++;
            _uk %= 1000;
            return _uk;
        }

        private int GetToken(int id)
        {
            var check = id % 7 + id % 3 + id % 2;
            var token = 1000 * check + id;
            return token;
        }

        private bool CheckToken(int token, out int id)
        {
            id = token % 1000;
            var checkFact = token / 1000;
            var check = id % 7 + id % 3 + id % 2;
            if(check != checkFact) return false;
            return true;
        }

        public bool Get(int token, out T data, bool consume = false, string receiver = "", string passcode = "")
        {
            data = default;

            if(!CheckToken(token, out int id)) return false;
            if(!_parcels.TryGetValue(id, out var parcel)) return false;
            if(parcel.Password != passcode) return false;

            if(consume)
            {
                Remove(parcel);
            }
            data = parcel.Data;
            return true;
        }

        public void RemoveAll(Predicate<ParcelT> func)
        {
            foreach(var parcel in _parcels.Values.ToList())
            {
                Remove(parcel);
            }
        }

        public void Clean()
        {
            foreach(var parcel in _parcels.Values.ToList())
            {
                if(DateTimeOffset.Now > parcel.ExpirationDate)
                {
                    Remove(parcel);
                }
            }
        }

        void IStorage.RemoveAll(Predicate<Parcel> func)
        {
            RemoveAll(func);
        }

        private void Remove(ParcelT parcel)
        {
            if(_parcels.TryRemove(parcel.Id, out var _))
            {
                if(parcel.Disposer != null)
                {
                    parcel.Disposer(parcel.Data);
                }
            }
        }
        
        public class ParcelT:Parcel
        {
            public T              Data;
            public Action<T>      Disposer;
        }
    }
    
    public class Parcel
    {
        public int            Id;
        public string         From;
        public string         Password;
        public DateTimeOffset ExpirationDate;
    }

    public interface IStorage
    {
        void Clean();
        void RemoveAll(Predicate<Parcel> func);
    }
}
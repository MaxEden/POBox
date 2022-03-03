using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace POBox
{
    public class StorageHub
    {
        private readonly ConcurrentDictionary<Type, IStorage> _storages = new ConcurrentDictionary<Type, IStorage>();

        private Storage<T> GetStorage<T>()
        {
            return (Storage<T>)GetStorage(typeof(T));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private IStorage GetStorage(Type type)
        {
            if(_storages.TryGetValue(type, out var obj))
            {
                return obj;
            }
            else
            {
                var instanceType = typeof(Storage<int>)
                    .GetGenericTypeDefinition()
                    .MakeGenericType(type);

                var instance = (IStorage)Activator.CreateInstance(instanceType);

                _storages.TryAdd(type, instance);
                return instance;
            }
        }

        public int Post<T>(T data, TimeSpan lifeTime, string from = "", string password = "", Action<T> disposer = null)
        {
            return GetStorage<T>().Post(data, lifeTime, disposer, from, password);
        }

        public bool Get<T>(int token, out T data, bool consume = false, string receiver = "", string password = "")
        {
            return GetStorage<T>().Get(token, out data, consume, receiver, password);
        }

        public void Clean()
        {
            foreach(var storage in _storages.Values)
            {
                storage.Clean();
            }
        }

        public void RemoveAll(Predicate<Parcel> func)
        {
            foreach(var storage in _storages.Values)
            {
                storage.RemoveAll(func);
            }
        }
        
        public void RemoveAll<T>(Predicate<Storage<T>.ParcelT> func)
        {
            GetStorage<T>().RemoveAll(func);
        }
        
        public void RemoveAllFrom(string from)
        {
            foreach(var storage in _storages.Values)
            {
                storage.RemoveAll(p=>p.From == from);
            }
        }
    }
}
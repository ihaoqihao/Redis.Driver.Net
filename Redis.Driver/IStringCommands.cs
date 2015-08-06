using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.Driver
{
    /// <summary>
    /// redis.strings命令
    /// </summary>
    /// <remarks>http://redis.io/commands#string</remarks>
    public interface IStringCommands
    {
        /// <summary>
        /// If key already exists and is a string, 
        /// this command appends the value at the end of the string. 
        /// If key does not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>the length of the string after the append operation.</returns>
        Task<int> Append(string key, string value, object asyncState = null);
        /// <summary>
        /// If key already exists and is a string, 
        /// this command appends the value at the end of the string. 
        /// If key does not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>the length of the string after the append operation.</returns>
        Task<int> Append(string key, byte[] value, object asyncState = null);
        /// <summary>
        /// When offset is beyond the string length, 
        /// the string is assumed to be a contiguous space with 0 bits. When key does not exist it is assumed to be an empty string, 
        /// so offset is always out of range and the value is also assumed to be a contiguous space with 0 bits.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="asyncState"></param>
        /// <returns>the bit value stored at offset.</returns>
        Task<int> GetBit(string key, int offset, object asyncState = null);
        /// <summary>
        /// Get the value of key. 
        /// If the key does not exist the special value nil is returned. 
        /// An error is returned if the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="asyncState"></param>
        /// <returns>the value of key, or nil when key does not exist.</returns>
        Task<byte[]> Get(string key, object asyncState = null);
        /// <summary>
        /// Get the value of key. 
        /// If the key does not exist the special value nil is returned. 
        /// An error is returned if the value stored at key is not a string, because GET only handles string values.        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <param name="asyncState"></param>
        /// <returns>the value of key, or nil when key does not exist.</returns>
        Task<T> Get<T>(string key, Func<byte[], T> valueFactory, object asyncState = null);
        /// <summary>
        /// Atomically sets key to value and returns the old value stored at key.
        /// Returns an error when key exists but does not hold a string value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task<byte[]> GetSet(string key, byte[] value, object asyncState = null);
        /// <summary>
        /// Atomically sets key to value and returns the old value stored at key.
        /// Returns an error when key exists but does not hold a string value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="valueFactory"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task<T> GetSet<T>(string key, byte[] value, Func<byte[], T> valueFactory, object asyncState = null);
        /// <summary>
        /// Returns the values of all specified keys. 
        /// For every key that does not hold a string value or does not exist, 
        /// the special value nil is returned. Because of this, the operation never fails.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="asyncState"></param>
        /// <returns>list of values at the specified keys.</returns>
        Task<byte[][]> Get(string[] keys, object asyncState = null);
        /// <summary>
        /// Returns the values of all specified keys. 
        /// For every key that does not hold a string value or does not exist, 
        /// the special value nil is returned. Because of this, the operation never fails.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="valueFactory"></param>
        /// <param name="asyncState"></param>
        /// <returns>list of values at the specified keys.</returns>
        Task<T[]> Get<T>(string[] keys, Func<byte[], T> valueFactory, object asyncState = null);
        /// <summary>
        /// Set key to hold the string value. 
        /// If key already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>always OK since SET can't fail.</returns>
        Task Set(string key, string value, object asyncState = null);
        /// <summary>
        /// 将值 value 关联到 key ，并将 key 的生存时间设为 seconds (以秒为单位)。
        /// 如果 key 已经存在， SETEX 命令将覆写旧值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirySeconds"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task Set(string key, string value, int expirySeconds, object asyncState = null);
        /// <summary>
        /// Set key to hold the string value. 
        /// If key already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>always OK since SET can't fail.</returns>
        Task Set(string key, byte[] value, object asyncState = null);
        /// <summary>
        /// 将值 value 关联到 key ，并将 key 的生存时间设为 seconds (以秒为单位)。
        /// 如果 key 已经存在， SETEX 命令将覆写旧值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirySeconds"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task Set(string key, byte[] value, int expirySeconds, object asyncState = null);
        /// <summary>
        /// Sets the given keys to their respective values. 
        /// MSET replaces existing values with new values, just as regular SET. See MSETNX if you don't want to overwrite existing values.
        /// MSET is atomic, so all given keys are set at once. 
        /// It is not possible for clients to see that some of the keys were updated while others are unchanged.
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="asyncState"></param>
        /// <returns>always OK since MSET can't fail.</returns>
        Task Set(Dictionary<string, byte[]> dic, object asyncState = null);
        /// <summary>
        /// Set key to hold string value if key does not exist. 
        /// In that case, it is equal to SET. 
        /// When key already holds a value, no operation is performed. 
        /// SETNX is short for "SET if N ot e X ists".
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task<bool> SetNX(string key, string value, object asyncState = null);
        /// <summary>
        /// Set key to hold string value if key does not exist. 
        /// In that case, it is equal to SET. 
        /// When key already holds a value, no operation is performed. 
        /// SETNX is short for "SET if N ot e X ists".
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task<bool> SetNX(string key, byte[] value, object asyncState = null);
    }
}
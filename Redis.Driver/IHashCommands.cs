using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.Driver
{
    /// <summary>
    /// hahs commands interface
    /// http://redis.io/commands#hash
    /// </summary>
    public interface IHashCommands
    {
        /// <summary>
        /// Removes the specified fields from the hash stored at key. 
        /// Non-existing fields are ignored. Non-existing keys are treated as empty hashes and this command returns 0.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task<bool> Remove(string key, string field, object asyncState = null);
        /// <summary>
        /// Removes the specified fields from the hash stored at key. 
        /// Non-existing fields are ignored. Non-existing keys are treated as empty hashes and this command returns 0.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task<int> Remove(string key, string[] fields, object asyncState = null);
        /// <summary>
        /// Returns if field is an existing field in the hash stored at key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="asyncState"></param>
        /// <returns>1 if the hash contains field. 0 if the hash does not contain field, or key does not exist.</returns>
        Task<bool> Exists(string key, string field, object asyncState = null);

        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="asyncState"></param>
        /// <returns>the value associated with field, or nil when field is not present in the hash or key does not exist.</returns>
        Task<byte[]> Get(string key, string field, object asyncState = null);
        /// <summary>
        /// Returns the values associated with the specified fields in the hash stored at key. 
        /// For every field that does not exist in the hash, a nil value is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <param name="asyncState"></param>
        /// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
        Task<byte[][]> Get(string key, string[] fields, object asyncState = null);
        /// <summary>
        /// Returns all fields and values of the hash stored at key. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="asyncState"></param>
        /// <returns>list of fields and their values stored in the hash, or an empty list when key does not exist.</returns>
        Task<Dictionary<string, byte[]>> GetAll(string key, object asyncState = null);

        /// <summary>
        /// Sets field in the hash stored at key to value. 
        /// If key does not exist, a new key holding a hash is created. 
        /// If field already exists in the hash, it is overwritten.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 
        /// 0 if field already exists in the hash and the value was updated.
        /// </returns>
        Task<bool> Set(string key, string field, string value, object asyncState = null);
        /// <summary>
        /// Sets field in the hash stored at key to value. 
        /// If key does not exist, a new key holding a hash is created.
        /// If field already exists in the hash, it is overwritten.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 
        /// 0 if field already exists in the hash and the value was updated.
        /// </returns>
        Task<bool> Set(string key, string field, byte[] value, object asyncState = null);
        /// <summary>
        /// Sets the specified fields to their respective values in the hash stored at key. 
        /// This command overwrites any existing fields in the hash. 
        /// If key does not exist, a new key holding a hash is created.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 
        /// 0 if field already exists in the hash and the value was updated.
        /// </returns>
        Task Set(string key, Dictionary<string, byte[]> values, object asyncState = null);
        /// <summary>
        /// Sets field in the hash stored at key to value, only if field does not yet exist. 
        /// If key does not exist, a new key holding a hash is created. If field already exists, this operation has no effect.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 
        /// 0 if field already exists in the hash and no operation was performed.
        /// </returns>
        Task<bool> SetIfNotExists(string key, string field, string value, object asyncState = null);
        /// <summary>
        /// Sets field in the hash stored at key to value, only if field does not yet exist. 
        /// If key does not exist, a new key holding a hash is created. 
        /// If field already exists, this operation has no effect.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 
        /// 0 if field already exists in the hash and no operation was performed.
        /// </returns>
        Task<bool> SetIfNotExists(string key, string field, byte[] value, object asyncState = null);
    }
}
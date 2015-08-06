using System.Threading.Tasks;

namespace Redis.Driver
{
    /// <summary>
    /// list commands
    /// </summary>
    public interface IListCommands
    {
        /// <summary>
        /// Insert all the specified values at the head of the list stored at key. 
        /// If key does not exist, it is created as empty list before performing the push operations. 
        /// When key holds a value that is not a list, an error is returned.
        /// It is possible to push multiple elements using a single command call just specifying multiple arguments at the end of the command. 
        /// Elements are inserted one after the other to the head of the list, from the leftmost element to the rightmost element. 
        /// So for instance the command LPUSH mylist a b c will result into a list containing c as first element, b as second element and a as third element.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>the length of the list after the push operations.</returns>
        Task<int> LPush(string key, string value, object asyncState = null);
        /// <summary>
        /// Insert all the specified values at the head of the list stored at key. 
        /// If key does not exist, it is created as empty list before performing the push operations. 
        /// When key holds a value that is not a list, an error is returned.
        /// It is possible to push multiple elements using a single command call just specifying multiple arguments at the end of the command. 
        /// Elements are inserted one after the other to the head of the list, from the leftmost element to the rightmost element. 
        /// So for instance the command LPUSH mylist a b c will result into a list containing c as first element, b as second element and a as third element.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="asyncState"></param>
        /// <returns>the length of the list after the push operations.</returns>
        Task<int> LPush(string key, string[] values, object asyncState = null);
        /// <summary>
        /// Insert all the specified values at the head of the list stored at key. 
        /// If key does not exist, it is created as empty list before performing the push operations. 
        /// When key holds a value that is not a list, an error is returned.
        /// It is possible to push multiple elements using a single command call just specifying multiple arguments at the end of the command. 
        /// Elements are inserted one after the other to the head of the list, from the leftmost element to the rightmost element. 
        /// So for instance the command LPUSH mylist a b c will result into a list containing c as first element, b as second element and a as third element.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>the length of the list after the push operations.</returns>
        Task<int> LPush(string key, byte[] value, object asyncState = null);
        /// <summary>
        /// Insert all the specified values at the head of the list stored at key. 
        /// If key does not exist, it is created as empty list before performing the push operations. 
        /// When key holds a value that is not a list, an error is returned.
        /// It is possible to push multiple elements using a single command call just specifying multiple arguments at the end of the command. 
        /// Elements are inserted one after the other to the head of the list, from the leftmost element to the rightmost element. 
        /// So for instance the command LPUSH mylist a b c will result into a list containing c as first element, b as second element and a as third element.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="asyncState"></param>
        /// <returns>the length of the list after the push operations.</returns>
        Task<int> LPush(string key, byte[][] values, object asyncState = null);
    }
}
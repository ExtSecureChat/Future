# ExtSecureChat Future Library [![Build Status](https://travis-ci.org/ExtSecureChat/Future.svg?branch=master)](https://travis-ci.org/ExtSecureChat/Future)
This project is inspired by JavaScript Promises, checkout this page: https://developer.mozilla.org/de/docs/Web/JavaScript/Reference/Global_Objects/Promise
It pretty much has the exact same syntax as Future.Promise

## Usage:
1. `using ExtSecureChat.Future`
2. Use one of the examples below

## Examples:

### Normal Promise (Resolve a value)
```c#
new Promise(() =>
{
    return "test";
}).Then(res =>
{
    Console.WriteLine(res); // Prints out: "test"
});
```

### Normal Promise (Catch an exception)
```c#
new Promise(() =>
{
    throw new Exception("exception");
}).Catch(err =>
{
    Console.WriteLine(err.message); // Prints out: "exception"
});
```

### Normal Promise (Combination)
```c#
new Promise(() =>
{
    if (someDynamicVariable == true)
    {
        return "test"
    }
    else
    {
        throw new Exception("exception");
    }
}).Then(res =>
{
    Console.WriteLine(res); // Prints out: "test" | if someDynamicVariable == true
}).Catch(err =>
{
    Console.WriteLine(err.message); // Prints out: "exception" | if someDynamicVariable == true
});
```

### Executor Promise (Resolve a value)
```c#
new Promise(resolve =>
{
    resolve("test");
}).Then(res =>
{
    Console.WriteLine(res); // Prints out: "test"
});
```

### Executor Promise (Catch an exception)
```c#
new Promise(reject =>
{
    reject(new Exception("exception"));
}).Catch(err =>
{
    Console.WriteLine(err.Message); // Prints out: "exception"
});
```
##### Note: Executor Promises will also catch exceptions thrown inside of the constructor, example:
```c#
new Promise(reject =>
{
    throw new Exception("exception1")); // <-- This will be catched by the Catch function, which makes the reject function pretty useless though, so it's highly advised not to use it
    
    reject(new Exception("exception2")); // Required for constructor
}).Catch(err =>
{
    Console.WriteLine(err.Message); // Prints out: "exception1"
});
```

### Executor Promise (Combination)
```c#
new Promise((resolve, reject) =>
{
    if (someDynamicVariable == true)
    {
        resolve("test");
    }
    else
    {
        reject(new Exception("exception"));
    }
}).Then(res =>
{
    Console.WriteLine(res); // Prints out: "test" | if someDynamicVariable == true
}).Catch(err =>
{
    Console.WriteLine(err.message); // Prints out: "exception" | if someDynamicVariable == true
});
```

##### Note to Executor Promises:
It is required to call all of the functions you are using in the constructor (e.g. resolve/reject). Otherwise an ambiguous error will be thrown!
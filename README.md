WindowsCronJobs
===============

A simple C#.Net app that will request a web page 1 or many times with intervals. Intented to be used as a Cron Job substitute on Windows platforms. Simply setup Task Scheduler to call this with some parameters and you're done.

This only exists because the Windows TaskScheduler dosn't support URLs. 

Usage
-----
You would usually have a different url, such as http://www.mywebsite.com/generateOutOfStockEmails.aspx or something like that, but in these example we're just using bing.com.

Request bing.com once, and exit.

    WindowsCronJobs http://www.bing.com

Request bing.com 5 times with a 2 second interval (2000 ms) and exit.

    WindowsCronJobs http://www.bing.com /c:5 /i:2000

Contributing
------------
Feel free to submit pull request and i'll accept them. This is intented to be a simple app.


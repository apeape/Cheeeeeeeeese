-------------------------------------------------------------------------------------------------------------------
STARKSOFT PROXY LIBRARY 

Included in this archive should be the following files.

Starksoft.Proxy.dll
Documentation.chm
Examples (VB and C#)
README.txt

The Starksoft Proxy Library is a software library built on the Microsoft .NET 2.0 Framework and coded in 100% 
C# managed code.  This library enables C# and VB.NET users to add proxy client capabilities to new or exisiting
.NET applications.

http://www.starksoft.com 

----------------------------------------------------------------------------------------------------------------------
ABOUT STARKSOFT PROXY LIBRARY

The Starksoft Proxy library provides classes that allow you as a developer create several types of client proxy connections
to a proxy server using the following protocols.

	* SOCKS v4
	* SOCKS v4a
	* SOCKS v5
	* HTTP Proxy

-----------------------------------------------------
REQUIREMENTS

    * Microsoft .NET Framework / Mono version 2.0 or higher.


-----------------------------------------------------
VERSION

1.0.2 - Added ability to specify TcpClient object on the constructors for the proxy objects.
	Also extended this ability to the factory object.
	Cleaned up a few issues with regards to exception reporting.


-----------------------------------------------------
VERSION

1.0.121 - Fixed Socks version 5 authentication bug.  Credentials were not being sent properly to the proxy server.

-----------------------------------------------------
MIT LICENSING

/*
 *  Authors:  Benton Stark
 * 
 *  Copyright (c) 2007-2009 Starksoft, LLC (http://www.starksoft.com) 
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
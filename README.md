OwinSSE
=======

How to send server events using Owin

In order to set up an SSE connection the server needs to keep the connection open after Invoke() has exited.

This is done by returning a TaskCompletionSource which can be completed/exceptioned 
later on when the client drops the connection

In addition the transfer-encoding header needs to be set to chunked so that owin self host will
not buffer the updates (still need to flush the stream though).



Protocol
======

The protocol is developed on top of the WebSocket protocol.
The protocol name for the WebSocket protocol must be `cell-attack-v0`.

Packets
------

### Connect Packet

Client -> Server:

```
0                8
+----------------+--------------
| Username Len   | Username...
+----------------+--------------
```


Server -> Client (may be sent multiple times):

```
0                8
+----------------+
| Status Code    |
+----------------+
```

##### Status Codes:

0. Connected, but waiting for game to start
1. Game starting
128. Uername already taken
129. Game already started

After a status code of `1`, no more connect packets can be sent.
After any status code greater than or equal to `127`, the server will close the connection.

### Client Spawn Packet

This packet is sent from the server to the client and defines which color the client is, and where the center of its spawn region is.

```
0          5     8                16               24               32
+----------+-----+----------------+----------------+----------------+
| Reserved | Clr | Center X...                                      :
+----------+-----+----------------+----------------+----------------+
: ...Center X...                                                    :
+----------+-----+----------------+----------------+----------------+
: ...Center X    | Center Y...                                      :
+----------+-----+----------------+----------------+----------------+
: ...Center Y...                                                    :
+----------+-----+----------------+----------------+----------------+
: ...Center Y    |
+----------------+
```

### Screen Size Packet

This packet is sent from the client to the server when the size of the view that needs to be sent of the field data has changed.

```
0                8                16               24               32
+----------------+----------------+----------------+----------------+
| Left X...                                                         :
+----------------+----------------+----------------+----------------+
: ...Left X                                                         |
+----------------+----------------+----------------+----------------+
| Top Y...                                                          :
+----------------+----------------+----------------+----------------+
: ...Top Y                                                          |
+----------------+----------------+----------------+----------------+
| Width                           | Height                          |
+----------------+----------------+----------------+----------------+
```

### Field Data Packet

This packet is sent from the server to the client when there is new data for it to render on the screen.

```
0                8                16               24               32
+----------------+----------------+----------------+----------------+
| Left X...                                                         :
+----------------+----------------+----------------+----------------+
: ...Left X                                                         |
+----------------+----------------+----------------+----------------+
| Top Y...                                                          :
+----------------+----------------+----------------+----------------+
: ...Top Y                                                          |
+----------------+----------------+----------------+----------------+
| Width                           | Height                          |
+----------------+----------------+----------------+----------------+
| Data...                                                           :
+----------------+----------------+----------------+----------------+
: ...                                                               :
+----------------+----------------+----------------+----------------+
```

Data is a stream of bytes scanning from left to right across the field, then moving from top to bottom.
Each byte follows the following format:

```
0          5     8
+----------+-----+
| Timer    | Clr |
+----------+-----+
```

The timer is the amount of time since the cell died, or `00000` if the cell is currently alive.
The color is `000` if the cell is dead and the timer has run out.

### Client Cell Update Packet

This packet is sent from the server to the client to update how many available cells it has to use.

```
0                8                16               24               32
+----------------+----------------+----------------+----------------+
| Number of Cells (IEEE float)                                      |
+----------------+----------------+----------------+----------------+
```

### Client Draw Packet

This packet is sent from the client to the server when the user draws a design to create with the client's available cells so the server can merge it into the field.

```
0                8                16               24               32
+----------------+----------------+----------------+----------------+
| Left X...                                                         :
+----------------+----------------+----------------+----------------+
: ...Left X                                                         |
+----------------+----------------+----------------+----------------+
| Top Y...                                                          :
+----------------+----------------+----------------+----------------+
: ...Top Y                                                          |
+----------------+----------------+----------------+----------------+
| Width                           | Height                          |
+----------------+----------------+----------------+----------------+
| Data...                                                           :
+----------------+----------------+----------------+----------------+
: ...                                                               :
+----------------+----------------+----------------+----------------+
```

Data is a stream of bits scanning from left to right across the field, then moving from top to bottom, where a `1` is a placed cell and a `0` is where cells should not be placed.
Extra `0`s will be padded to the end of the data to align it to a byte.

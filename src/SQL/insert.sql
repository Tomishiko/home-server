insert into roles(name)
values ('user'), ('manager');

insert into users(uname,password,role_id)
values
    ('user1','sdfjn1',1),
    ('user2','qwety',1),
    ('user3','qwetyasd',1),
    ('admin','AQAAAAIAAYagAAAAEPePPB2Ye/oMAFYr02HulXU8xnQjBGC76OJw1ldGcPYIGmYPz3nCM/a+vS6ggqawHA==',2), -- "admin" "admin"
    ('user4','qwesdfatyasd',1);



insert Into logs (user_id,username,eventname,time)
values
    (1,'user2','eventevent2','2000-01-08 04:05:06'),
    (3,'user3','eventevent3','2001-01-08 04:05:06'),
    (4,'user4','eventevent4','2002-01-08 04:05:06'),
    (1,'user5','eventevent5','2003-01-08 04:05:06'),
    (2,'user6','eventevent6','2004-01-08 04:05:06'),
    (3,'user7','eventevent7','2005-01-08 04:05:06');

insert into roles(name)
values ('user'), ('manager');

insert into users(uname,password,email,role_id)
values
    ('user1','sdfjn1','test@email.com',1),
    ('user2','qwety','test@email.com',1),
    ('user3','qwetyasd','test@email.com',1),
    ('user4','qwesdfatyasd','test@email.com',1),
    -- ↑ these are for testing only

    ('admin','AQAAAAIAAYagAAAAEPePPB2Ye/oMAFYr02HulXU8xnQjBGC76OJw1ldGcPYIGmYPz3nCM/a+vS6ggqawHA==','admin@email.com',2); -- "admin" "admin"



insert Into logs (username,eventname,time)
values
    ('user2','eventevent2','2000-01-08 04:05:06'),
    ('user3','eventevent3','2001-01-08 04:05:06'),
    ('user4','eventevent4','2002-01-08 04:05:06'),
    ('user5','eventevent5','2003-01-08 04:05:06'),
    ('user6','eventevent6','2004-01-08 04:05:06'),
    ('user7','eventevent7','2005-01-08 04:05:06');

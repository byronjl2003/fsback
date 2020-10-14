CREATE TABLE user
(
    email varchar(255) NOT NULL,
    name varchar
    (255) NOT NULL,
    pass varchar
    (255) NOT NULL,
    PRIMARY KEY
    (email)
)
ENGINE=INNODB;

CREATE TABLE files
(
    id int NOT NULL
    AUTO_INCREMENT,
    name varchar
    (255) NOT NULL,
    path varchar
    (1024) not null,
    id_user varchar
    (255) NOT NULL,
    PRIMARY KEY
    (id),
    INDEX par_ind
    (id_user),
    FOREIGN KEY
    (id_user)
    REFERENCES user
    (email)
    ON
    DELETE CASCADE 


)ENGINE=INNODB;

    insert into user
        (email,name,pass)
    values('byronjl2003@gmail.com', 'ByronLopez', '11001100');
    insert into user
        (email,name,pass)
    values('byronjl2003@hotmail.com', 'JoseLopez', '123456789');
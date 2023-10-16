USE Running_shop

INSERT INTO colors(name) VALUES('Red'),('Blue'),('White'),('Black')

SELECT * FROM colors

INSERT INTO genders(name) VALUES('Male'),('Female'),('Unisex')

SELECT * FROM genders

INSERT INTO genders(name) VALUES('Male'),('Female'),('Unisex')

SELECT * FROM users

INSERT INTO users(fullname, email, avatar, tel, address, city, passwordHash, role) 
VALUES('VIET ANH', 'dovietanh2k@gmail.com', 'https://www.w3schools.com/howto/img_avatar.png', 0979278962, 'MaiDinh-SocSon-HaNoi', 'Ha Noi', '12345678', 'ADMIN')

USE Running_shop
SELECT * FROM categories
INSERT INTO categories(name) VALUES('smartphone'),('television'),('fashion')

SELECT * FROM size
INSERT INTO size(name) VALUES('X'),('L'),('XL')

SELECT * FROM brand
INSERT INTO brand(name) VALUES('Nice'),('Gucci'),('Sam sung')


SELECT * FROM products
SELECT * FROM categories

INSERT INTO products(name, price, description, thumbnail, qty, category_id, createDate, user_id, gender_id, brand_id, size_id, color_id) 
VALUES('shoes 4', 1000, 'common shoes 4', 'https://www.shutterstock.com/image-photo/pair-pink-sport-shoes-on-260nw-228691018.jpg',5, 3, GETDATE(), 7, 1, 1, 1, 1),
('shoes 5', 1500, 'common shoes 5', 'https://www.shutterstock.com/image-photo/pair-pink-sport-shoes-on-260nw-228691018.jpg',6, 3, GETDATE(), 7, 2, 2, 2, 2),
(' shoes 6', 2000, 'common shoes... 6', 'https://www.shutterstock.com/image-photo/pair-pink-sport-shoes-on-260nw-228691018.jpg',7, 3, GETDATE(), 7, 3, 4, 3, 3)


ALTER TABLE users ALTER COLUMN passwordHash BINARY(32) NOT NULL;

EXEC sp_rename 'users.password', 'passwordHash', 'COLUMN';

UPDATE users SET passwordHash = '' WHERE id = 1;

ALTER TABLE users ALTER COLUMN password BINARY(32) NOT NULL

SELECT * FROM users

ALTER TABLE users DROP COLUMN password;

DELETE FROM users where id = 14;



--DELETE FROM products;

DELETE FROM users where id = 20;
DELETE FROM users where id = 22;
DELETE FROM users where id = 23;


ALTER TABLE users ADD passwordHash BINARY(32) NOT NULL;

ALTER TABLE users ADD passwordSalt BINARY(32) NOT NULL;


ALTER TABLE users ALTER COLUMN passwordHash BINARY(64) NOT NULL
USE Running_shop
ALTER TABLE users ALTER COLUMN passwordSalt BINARY(128) NOT NULL

ALTER TABLE users ADD verificationToken VARCHAR(350) NOT NULL;

ALTER TABLE users ADD verifiedAt Datetime;

ALTER TABLE users ADD passwordResetToken VARCHAR(350);

ALTER TABLE users ADD ResetTokenExpires Datetime;

INSERT INTO users( fullname, email, role, passwordHash, passwordSalt, verificationToken )
VALUES ('tuan', 'anhtoankieu', 'ADMIN',0x0123456789ABCDEF, 0x0123456789ABCDEF, 'dfghjkcvbnmrtyu123456');

SELECT * FROM products


SELECT * FROM users


ALTER TABLE users ADD TokenExpired Datetime;

EXEC sp_rename 'users.verificationToken', 'refreshToken', 'COLUMN';
EXEC sp_rename 'users.verifiedAt', 'TokenCreated', 'COLUMN';

ALTER TABLE users ALTER COLUMN refreshToken VARCHAR(350);

SELECT * FROM users where email = 'user@example.com';

USE Running_shop
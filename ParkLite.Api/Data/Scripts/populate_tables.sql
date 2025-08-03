PRAGMA foreign_keys
= ON;

-- Accounts
INSERT INTO Accounts
	(Name, IsActive)
VALUES
	('Smith Family', 1),
	('Johnson Family', 1),
	('Williams Family', 1),
	('Brown Family', 1),
	('Jones Family', 1),
	('Garcia Family', 1),
	('Miller Family', 1),
	('Davis Family', 1),
	('Rodriguez Family', 1),
	('Martinez Family', 1);

-- Contacts
INSERT INTO Contacts
	(AccountId, Name, Phone)
VALUES
	(1, 'John Smith', '555-1234'),
	(1, 'Jane Smith', '555-5678'),
	(2, 'Alice Johnson', '555-2345'),
	(3, 'Bob Williams', '555-3456'),
	(4, 'Carol Brown', '555-4567'),
	(5, 'Dave Jones', '555-5678'),
	(6, 'Eva Garcia', '555-6789'),
	(7, 'Frank Miller', '555-7890'),
	(8, 'Grace Davis', '555-8901'),
	(9, 'Hank Rodriguez', '555-9012'),
	(10, 'Ivy Martinez', '555-0123');

-- Vehicles
INSERT INTO Vehicles
	(AccountId, Plate, Model)
VALUES
	(1, 'ABC123', 'Toyota Camry'),
	(1, 'XYZ789', 'Honda Accord'),
	(2, 'JKL456', 'Ford Focus'),
	(3, 'MNO321', 'Chevy Malibu'),
	(4, 'PQR654', 'Nissan Altima'),
	(5, 'STU987', 'Hyundai Elantra'),
	(6, 'VWX543', 'Kia Soul'),
	(7, 'DEF678', 'Mazda CX-5'),
	(8, 'GHI321', 'Tesla Model 3'),
	(9, 'LMN999', 'BMW 3 Series'),
	(10, 'OPQ888', 'Audi A4');

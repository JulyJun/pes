CREATE SCHEMA HumidTempBoard;
use HumidTempBoard;
CREATE TABLE room(
                    idx           INT   NOT NULL auto_increment,
                    co2           FLOAT NOT NULL DEFAULT -1.0,
                    co2_approx    ENUM('LOW', 'MEDIUM', 'HIGH') NOT NULL DEFAULT 'MEDIUM',
                    gas           FLOAT NOT NULL DEFAULT -1.0,
                    gas_approx    ENUM('LOW', 'MEDIUM', 'HIGH') NOT NULL DEFAULT 'MEDIUM',
                    temp          FLOAT NOT NULL,
                    temp_approx   ENUM('LOW', 'MEDIUM', 'HIGH') NOT NULL DEFAULT 'MEDIUM',
                    humid         FLOAT NOT NULL,
                    humid_approx  ENUM('LOW', 'MEDIUM', 'HIGH') NOT NULL DEFAULT 'MEDIUM',
                    room_occupied ENUM('Y', 'N') DEFAULT 'N',
                    outdoor_temp  FLOAT NOT NULL,
                    outdoor_humid FLOAT NOT NULL,
                    update_time   DATETIME NOT NULL DEFAULT NOW(),
                    PRIMARY KEY(idx)
);

ALTER TABLE room
ADD COLUMN update_time DATETIME NOT NULL DEFAULT NOW();
SELECT *
FROM room;

/* MAX_ROWS, AVG_ROW_LENGTH의 원하는 사이즈 지정하기 */
 ALTER TABLE room MAX_ROWS = n AVG_ROW_LENGTH = n ;
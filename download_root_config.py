import ftplib
import os

try:
    ftp = ftplib.FTP('windows11.aspone.cz')
    ftp.login('EkoBio.org_lordkikin', 'Brzsilpot7!')
    ftp.cwd('/www')
    with open('root_web.config', 'wb') as f:
        ftp.retrbinary('RETR web.config', f.write)
    ftp.quit()
    print('Successful')
except Exception as e:
    print(f'Error: {e}')

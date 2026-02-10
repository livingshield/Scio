import ftplib
import os

# FTP Settings
FTP_SERVER = "windows11.aspone.cz"
FTP_USERNAME = "EkoBio.org_lordkikin"
FTP_PASSWORD = "Brzsilpot7!"
REMOTE_DIR = "/www/scio"

def main():
    try:
        ftp = ftplib.FTP(FTP_SERVER)
        ftp.login(FTP_USERNAME, FTP_PASSWORD)
        ftp.set_pasv(True)
        
        ftp.cwd(REMOTE_DIR)
        lines = []
        ftp.retrlines('LIST', lines.append)
        
        count = 0
        for line in lines:
            parts = line.split()
            if not parts: continue
            is_dir = line.startswith('d')
            if not is_dir:
                count += 1
        
        print(f"RESULT_COUNT: {count}")
        
        ftp.quit()
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()

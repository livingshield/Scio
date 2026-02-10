import ftplib

# FTP Settings
FTP_SERVER = "windows11.aspone.cz"
FTP_USERNAME = "EkoBio.org_lordkikin"
FTP_PASSWORD = "Brzsilpot7!"
REMOTE_FILE = "/www/scio/web.config"

def main():
    try:
        ftp = ftplib.FTP(FTP_SERVER)
        ftp.login(FTP_USERNAME, FTP_PASSWORD)
        ftp.set_pasv(True)
        
        print(f"Reading {REMOTE_FILE}...")
        ftp.retrlines(f"RETR {REMOTE_FILE}", lambda line: print(line))
        
        ftp.quit()
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()

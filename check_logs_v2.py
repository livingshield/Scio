import ftplib

# FTP Settings
FTP_SERVER = "windows11.aspone.cz"
FTP_USERNAME = "EkoBio.org_lordkikin"
FTP_PASSWORD = "Brzsilpot7!"
REMOTE_LOG_DIR = "/www/scio/logs"

def main():
    try:
        ftp = ftplib.FTP(FTP_SERVER)
        ftp.login(FTP_USERNAME, FTP_PASSWORD)
        ftp.set_pasv(True)
        
        print(f"Checking {REMOTE_LOG_DIR}...")
        try:
            ftp.cwd(REMOTE_LOG_DIR)
            lines = []
            ftp.retrlines('LIST', lines.append)
            
            for line in lines:
                parts = line.split()
                if not parts: continue
                filename = " ".join(parts[8:])
                print(f"Found: {filename}")
                if filename.startswith("stdout"):
                    print(f"--- CONTENT OF {filename} ---")
                    ftp.retrlines(f"RETR {filename}", lambda l: print(l))
        except Exception as e:
            print(f"Logs folder error: {e}")
            
        ftp.quit()
    except Exception as e:
        print(f"FTP error: {e}")

if __name__ == "__main__":
    main()

import ftplib

# FTP Settings
FTP_SERVER = "windows11.aspone.cz"
FTP_USERNAME = "EkoBio.org_lordkikin"
FTP_PASSWORD = "Brzsilpot7!"
REMOTE_LOG_DIR = "/www/scio/logs"

def list_logs():
    try:
        print(f"Connecting to {FTP_SERVER} and checking {REMOTE_LOG_DIR}...")
        ftp = ftplib.FTP(FTP_SERVER)
        ftp.login(FTP_USERNAME, FTP_PASSWORD)
        ftp.set_pasv(True)
        
        try:
            ftp.cwd(REMOTE_LOG_DIR)
            files = []
            ftp.retrlines('LIST', files.append)
            
            if not files:
                print("No log files found in /logs directory.")
            else:
                print("\nFound log files:")
                for f in files:
                    print(f)
                    # If it's a file, let's try to read its content
                    # Format is usually 'stdout_YYYYMMDDHHMMSS_PID.log'
                    parts = f.split()
                    filename = parts[-1]
                    if filename.endswith(".log"):
                        print(f"\n--- Content of {filename} ---")
                        ftp.retrlines(f"RETR {filename}", lambda line: print(line))
                        print("-" * (len(filename) + 20))
        except ftplib.error_perm as e:
            print(f"Could not access logs directory: {e}")
            
        ftp.quit()
    except Exception as e:
        print(f"Error checking logs: {e}")

if __name__ == "__main__":
    list_logs()

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
        
        print(f"Deleting {REMOTE_FILE}...")
        try:
            ftp.delete(REMOTE_FILE)
            print("Deleted.")
        except:
            print("Already deleted or not found.")
            
        print(f"Uploading new web.config...")
        with open("./publish/web.config", "rb") as f:
            ftp.storbinary(f"STOR {REMOTE_FILE}", f)
        print("Uploaded.")
        
        ftp.quit()
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""Save analysis results to files"""
import pymssql
import ftplib

# === DATABASE ANALYSIS ===
with open("db_analysis.txt", "w", encoding="utf-8") as f:
    conn = pymssql.connect(server="sql8.aspone.cz", user="db4937", password="lordkikin", database="db4937")
    cursor = conn.cursor()
    
    f.write("="*60 + "\n")
    f.write("DATABASE SCHEMA ANALYSIS\n")
    f.write("="*60 + "\n\n")
    
    f.write("1. USERS TABLE STRUCTURE:\n")
    cursor.execute("""
        SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE, COLUMN_DEFAULT
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = 'Users'
        ORDER BY ORDINAL_POSITION
    """)
    
    for col_name, data_type, max_len, nullable, default in cursor.fetchall():
        len_str = f"({max_len})" if max_len else ""
        null_str = "NULL" if nullable == "YES" else "NOT NULL"
        default_str = f" DEFAULT {default}" if default else ""
        f.write(f"  {col_name:20} {data_type}{len_str:15} {null_str:10}{default_str}\n")
    
    cursor.execute("SELECT COUNT(*) FROM Users")
    user_count = cursor.fetchone()[0]
    f.write(f"\n  Total rows: {user_count}\n")
    
    f.write("\n2. INDEXES:\n")
    cursor.execute("""
        SELECT i.name AS index_name, i.type_desc, COL_NAME(ic.object_id, ic.column_id) AS column_name
        FROM sys.indexes i
        INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        WHERE OBJECT_NAME(i.object_id) = 'Users'
        ORDER BY i.name, ic.key_ordinal
    """)
    current = None
    for idx_name, idx_type, col_name in cursor.fetchall():
        if idx_name != current:
            f.write(f"  {idx_name} ({idx_type}):\n")
            current = idx_name
        f.write(f"    - {col_name}\n")
    
    f.write("\n3. EF MIGRATIONS:\n")
    cursor.execute("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory'")
    has_ef = cursor.fetchone()[0]
    f.write(f"  Migration table: {'EXISTS' if has_ef > 0 else 'NOT FOUND'}\n")
    
    if has_ef > 0:
        cursor.execute("SELECT COUNT(*) FROM __EFMigrationsHistory")
        f.write(f"  Applied migrations: {cursor.fetchone()[0]}\n")
    
    conn.close()

print("✓ Database analysis saved to: db_analysis.txt")

# === FTP ANALYSIS ===
with open("ftp_analysis.txt", "w", encoding="utf-8") as f:
    ftp = ftplib.FTP("windows11.aspone.cz", timeout=30)
    ftp.login("EkoBio.org_lordkikin", "Brzsilpot7!")
    
    f.write("="*60 + "\n")
    f.write("FTP STRUCTURE ANALYSIS\n")
    f.write("="*60 + "\n\n")
    
    def explore(path, depth=0):
        if depth > 2:
            return
        indent = "  " * depth
        try:
            ftp.cwd(path)
            f.write(f"{indent}📁 {path}\n")
            items = []
            ftp.dir(items.append)
            
            dirs = []
            files = []
            for item in items:
                parts = item.split()
                if len(parts) < 9:
                    continue
                name = " ".join(parts[8:])
                if item.startswith('d'):
                    dirs.append(name)
                else:
                    files.append((name, parts[4]))
            
            for name, size in files:
                f.write(f"{indent}  📄 {name} ({size} bytes)\n")
            
            for dir_name in dirs:
                if dir_name not in ['.', '..']:
                    try:
                        explore(f"{path}/{dir_name}", depth + 1)
                    except:
                        pass
            
            if path != "/":
                ftp.cwd("..")
        except Exception as e:
            f.write(f"{indent}⚠️  Error: {e}\n")
    
    explore("/", 0)
    
    f.write("\n" + "="*60 + "\n")
    f.write("DEPLOYMENT PATHS CHECK\n")
    f.write("="*60 + "\n")
    
    for path in ["/www", "/www/wwwroot", "/wwwroot", "/public_html"]:
        try:
            ftp.cwd(path)
            items = []
            ftp.dir(items.append)
            f.write(f"✓ {path} (items: {len(items)})\n")
            ftp.cwd("/")
        except:
            f.write(f"✗ {path} (not found)\n")
    
    ftp.quit()

print("✓ FTP analysis saved to: ftp_analysis.txt")
print("\nDone!")

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BoonieBear.DeckUnit.DAL.SqliteDAL;

namespace BoonieBear.DeckUnit.DAL
{
    enum DBType
    {
        Sqlite,
        MysqlL,
        Mssql,
    }
    /// <summary>
    /// 数据库访问工厂类
    /// </summary>
    public class DALFactory
    {
        static ISqlite CreateDAL(DBType dbType)
        {
            switch (dbType)
            {
                case DBType.Sqlite:
                    return new IsqliteSqlDAL();
                default:
                    return new IsqliteSqlDAL();
            }
        }
    }
}

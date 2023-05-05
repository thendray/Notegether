﻿using Notegether.Bll.Models;
using Notegether.Dal.Queries;

namespace Notegether.Dal;

public interface IUserRepository
{
    public Task AddUser(AddUserQuery query);
}
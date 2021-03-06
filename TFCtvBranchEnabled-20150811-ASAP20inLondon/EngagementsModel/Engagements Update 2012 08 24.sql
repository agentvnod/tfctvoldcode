/****** Object:  Table [dbo].[ShowReactionSummary]    Script Date: 08/24/2012 10:20:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShowReactionSummary](
	[CategoryId] [int] NOT NULL,
	[ReactionTypeId] [int] NOT NULL,
	[Total] [int] NOT NULL,
	[Total7Days] [int] NOT NULL,
	[Total30Days] [int] NOT NULL,
 CONSTRAINT [PK_ShowSummary] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC,
	[ReactionTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EpisodeReactionSummary]    Script Date: 08/24/2012 10:20:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EpisodeReactionSummary](
	[EpisodeId] [int] NOT NULL,
	[ReactionTypeId] [int] NOT NULL,
	[Total] [int] NOT NULL,
	[Total7Days] [int] NOT NULL,
	[Total30Days] [int] NOT NULL,
 CONSTRAINT [PK_EpisodeSummary] PRIMARY KEY CLUSTERED 
(
	[EpisodeId] ASC,
	[ReactionTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CelebrityReactionSummary]    Script Date: 08/24/2012 10:20:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CelebrityReactionSummary](
	[CelebrityId] [int] NOT NULL,
	[ReactionTypeId] [int] NOT NULL,
	[Total] [int] NOT NULL,
	[Total7Days] [int] NOT NULL,
	[Total30Days] [int] NOT NULL,
 CONSTRAINT [PK_CelebritySummary] PRIMARY KEY CLUSTERED 
(
	[CelebrityId] ASC,
	[ReactionTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[ShowReactionSummaryTotal]    Script Date: 08/24/2012 10:20:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[ShowReactionSummaryTotal] as
begin

declare @categoryId int
declare @reactionTypeId int
declare @count int

declare ShowReaction_Cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY STATIC 
FOR
select categoryId, reactionTypeId, COUNT(*)
from ShowReactions 
group by CategoryId,ReactionTypeId 


open ShowReaction_Cursor
fetch next from ShowReaction_Cursor into @categoryId, @reactionTypeId, @count

while @@FETCH_STATUS = 0
begin
    update ShowReactionSummary 
    set total = @count 
    where CategoryId=@categoryId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into ShowReactionSummary (CategoryId, ReactionTypeId, Total)
         values (@categoryId,@reactionTypeId,@count)
	fetch next from ShowReaction_Cursor into @categoryId, @reactionTypeId, @count
end 

close ShowReaction_Cursor
deallocate  ShowReaction_Cursor

end
GO
/****** Object:  StoredProcedure [dbo].[ShowReactionSummary7Days]    Script Date: 08/24/2012 10:20:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[ShowReactionSummary7Days] as
begin

declare @categoryId int
declare @reactionTypeId int
declare @count int
declare @now datetime
set @now = DateAdd(d,-7,GETDATE())

declare ShowReaction_Cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY STATIC 
FOR
select categoryId, reactionTypeId, COUNT(*)
from ShowReactions 
where DateTime > @now
group by CategoryId,ReactionTypeId 


open ShowReaction_Cursor
fetch next from ShowReaction_Cursor into @categoryId, @reactionTypeId, @count

while @@FETCH_STATUS = 0
begin
    update ShowReactionSummary 
    set total7days = @count 
    where CategoryId=@categoryId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into ShowReactionSummary (CategoryId, ReactionTypeId, Total7Days)
         values (@categoryId,@reactionTypeId,@count)
	fetch next from ShowReaction_Cursor into @categoryId, @reactionTypeId, @count
end 

close ShowReaction_Cursor
deallocate  ShowReaction_Cursor

end
GO
/****** Object:  StoredProcedure [dbo].[ShowReactionSummary30Days]    Script Date: 08/24/2012 10:20:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[ShowReactionSummary30Days] as
begin

declare @categoryId int
declare @reactionTypeId int
declare @count int
declare @now datetime
set @now = DateAdd(d,-30,GETDATE())

declare ShowReaction_Cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY STATIC 
FOR
select categoryId, reactionTypeId, COUNT(*)
from ShowReactions 
where DateTime > @now
group by CategoryId,ReactionTypeId 


open ShowReaction_Cursor
fetch next from ShowReaction_Cursor into @categoryId, @reactionTypeId, @count

while @@FETCH_STATUS = 0
begin
    update ShowReactionSummary 
    set total30days = @count 
    where CategoryId=@categoryId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into ShowReactionSummary (CategoryId, ReactionTypeId, Total30Days)
         values (@categoryId,@reactionTypeId,@count)
	fetch next from ShowReaction_Cursor into @categoryId, @reactionTypeId, @count
end 

close ShowReaction_Cursor
deallocate  ShowReaction_Cursor

end
GO
/****** Object:  StoredProcedure [dbo].[EpisodeReactionSummaryTotal]    Script Date: 08/24/2012 10:20:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[EpisodeReactionSummaryTotal] as
begin

declare @EpisodeId int
declare @reactionTypeId int
declare @count int

declare EpisodeReaction_Cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY STATIC 
FOR
select EpisodeId, reactionTypeId, COUNT(*)
from EpisodeReactions 
group by EpisodeId,ReactionTypeId 


open EpisodeReaction_Cursor
fetch next from EpisodeReaction_Cursor into @EpisodeId, @reactionTypeId, @count

while @@FETCH_STATUS = 0
begin
    update EpisodeReactionSummary 
    set total = @count 
    where EpisodeId=@EpisodeId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into EpisodeReactionSummary (EpisodeId, ReactionTypeId, Total)
         values (@EpisodeId,@reactionTypeId,@count)
	fetch next from EpisodeReaction_Cursor into @EpisodeId, @reactionTypeId, @count
end 

close EpisodeReaction_Cursor
deallocate  EpisodeReaction_Cursor

end
GO
/****** Object:  StoredProcedure [dbo].[EpisodeReactionSummary7Days]    Script Date: 08/24/2012 10:20:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[EpisodeReactionSummary7Days] as
begin

declare @EpisodeId int
declare @reactionTypeId int
declare @count int
declare @now datetime
set @now = DateAdd(d,-7,GETDATE())

declare EpisodeReaction_Cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY STATIC 
FOR
select EpisodeId, reactionTypeId, COUNT(*)
from EpisodeReactions 
where DateTime > @now
group by EpisodeId,ReactionTypeId 


open EpisodeReaction_Cursor
fetch next from EpisodeReaction_Cursor into @EpisodeId, @reactionTypeId, @count

while @@FETCH_STATUS = 0
begin
    update EpisodeReactionSummary 
    set total7Days = @count 
    where EpisodeId=@EpisodeId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into EpisodeReactionSummary (EpisodeId, ReactionTypeId, Total7Days)
         values (@EpisodeId,@reactionTypeId,@count)
	fetch next from EpisodeReaction_Cursor into @EpisodeId, @reactionTypeId, @count
end 

close EpisodeReaction_Cursor
deallocate  EpisodeReaction_Cursor

end
GO
/****** Object:  StoredProcedure [dbo].[EpisodeReactionSummary30Days]    Script Date: 08/24/2012 10:20:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[EpisodeReactionSummary30Days] as
begin

declare @EpisodeId int
declare @reactionTypeId int
declare @count int
declare @now datetime
set @now = DateAdd(d,-30,GETDATE())

declare EpisodeReaction_Cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY STATIC 
FOR
select EpisodeId, reactionTypeId, COUNT(*)
from EpisodeReactions 
where DateTime > @now
group by EpisodeId,ReactionTypeId 


open EpisodeReaction_Cursor
fetch next from EpisodeReaction_Cursor into @EpisodeId, @reactionTypeId, @count

while @@FETCH_STATUS = 0
begin
    update EpisodeReactionSummary 
    set total30Days = @count 
    where EpisodeId=@EpisodeId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into EpisodeReactionSummary (EpisodeId, ReactionTypeId, Total30Days)
         values (@EpisodeId,@reactionTypeId,@count)
	fetch next from EpisodeReaction_Cursor into @EpisodeId, @reactionTypeId, @count
end 

close EpisodeReaction_Cursor
deallocate  EpisodeReaction_Cursor

end
GO
/****** Object:  StoredProcedure [dbo].[CelebrityReactionSummaryTotal]    Script Date: 08/24/2012 10:20:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[CelebrityReactionSummaryTotal] as
begin

declare @CelebrityId int
declare @reactionTypeId int
declare @count int

declare CelebrityReaction_Cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY STATIC 
FOR
select CelebrityId, reactionTypeId, COUNT(*)
from CelebrityReactions 
group by CelebrityId,ReactionTypeId 


open CelebrityReaction_Cursor
fetch next from CelebrityReaction_Cursor into @CelebrityId, @reactionTypeId, @count

while @@FETCH_STATUS = 0
begin
    update CelebrityReactionSummary 
    set total = @count 
    where CelebrityId=@CelebrityId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into CelebrityReactionSummary (CelebrityId, ReactionTypeId, Total)
         values (@CelebrityId,@reactionTypeId,@count)
	fetch next from CelebrityReaction_Cursor into @CelebrityId, @reactionTypeId, @count
end 

close CelebrityReaction_Cursor
deallocate  CelebrityReaction_Cursor

end
GO
/****** Object:  StoredProcedure [dbo].[CelebrityReactionSummary7Days]    Script Date: 08/24/2012 10:20:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[CelebrityReactionSummary7Days] as
begin

declare @CelebrityId int
declare @reactionTypeId int
declare @count int
declare @now datetime
set @now = DateAdd(d,-7,GETDATE())

declare CelebrityReaction_Cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY STATIC 
FOR
select CelebrityId, reactionTypeId, COUNT(*)
from CelebrityReactions 
where DateTime > @now
group by CelebrityId,ReactionTypeId 


open CelebrityReaction_Cursor
fetch next from CelebrityReaction_Cursor into @CelebrityId, @reactionTypeId, @count

while @@FETCH_STATUS = 0
begin
    update CelebrityReactionSummary 
    set total7Days = @count 
    where CelebrityId=@CelebrityId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into CelebrityReactionSummary (CelebrityId, ReactionTypeId, Total7Days)
         values (@CelebrityId,@reactionTypeId,@count)
	fetch next from CelebrityReaction_Cursor into @CelebrityId, @reactionTypeId, @count
end 

close CelebrityReaction_Cursor
deallocate  CelebrityReaction_Cursor

end
GO
/****** Object:  StoredProcedure [dbo].[CelebrityReactionSummary30Days]    Script Date: 08/24/2012 10:20:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[CelebrityReactionSummary30Days] as
begin

declare @CelebrityId int
declare @reactionTypeId int
declare @count int
declare @now datetime
set @now = DateAdd(d,-30,GETDATE())

declare CelebrityReaction_Cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY STATIC 
FOR
select CelebrityId, reactionTypeId, COUNT(*)
from CelebrityReactions 
where DateTime > @now
group by CelebrityId,ReactionTypeId 


open CelebrityReaction_Cursor
fetch next from CelebrityReaction_Cursor into @CelebrityId, @reactionTypeId, @count

while @@FETCH_STATUS = 0
begin
    update CelebrityReactionSummary 
    set total30Days = @count 
    where CelebrityId=@CelebrityId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into CelebrityReactionSummary (CelebrityId, ReactionTypeId, Total30Days)
         values (@CelebrityId,@reactionTypeId,@count)
	fetch next from CelebrityReaction_Cursor into @CelebrityId, @reactionTypeId, @count
end 

close CelebrityReaction_Cursor
deallocate  CelebrityReaction_Cursor

end
GO
/****** Object:  Default [DF_CelebritySummary_Total]    Script Date: 08/24/2012 10:20:15 ******/
ALTER TABLE [dbo].[CelebrityReactionSummary] ADD  CONSTRAINT [DF_CelebritySummary_Total]  DEFAULT ((0)) FOR [Total]
GO
/****** Object:  Default [DF_CelebritySummary_Total7Days]    Script Date: 08/24/2012 10:20:15 ******/
ALTER TABLE [dbo].[CelebrityReactionSummary] ADD  CONSTRAINT [DF_CelebritySummary_Total7Days]  DEFAULT ((0)) FOR [Total7Days]
GO
/****** Object:  Default [DF_CelebritySummary_Total30Days]    Script Date: 08/24/2012 10:20:15 ******/
ALTER TABLE [dbo].[CelebrityReactionSummary] ADD  CONSTRAINT [DF_CelebritySummary_Total30Days]  DEFAULT ((0)) FOR [Total30Days]
GO
/****** Object:  Default [DF_EpisodeSummary_Total]    Script Date: 08/24/2012 10:20:15 ******/
ALTER TABLE [dbo].[EpisodeReactionSummary] ADD  CONSTRAINT [DF_EpisodeSummary_Total]  DEFAULT ((0)) FOR [Total]
GO
/****** Object:  Default [DF_EpisodeSummary_Total7Days]    Script Date: 08/24/2012 10:20:15 ******/
ALTER TABLE [dbo].[EpisodeReactionSummary] ADD  CONSTRAINT [DF_EpisodeSummary_Total7Days]  DEFAULT ((0)) FOR [Total7Days]
GO
/****** Object:  Default [DF_EpisodeSummary_Total30Days]    Script Date: 08/24/2012 10:20:15 ******/
ALTER TABLE [dbo].[EpisodeReactionSummary] ADD  CONSTRAINT [DF_EpisodeSummary_Total30Days]  DEFAULT ((0)) FOR [Total30Days]
GO
/****** Object:  Default [DF_ShowSummary_Total]    Script Date: 08/24/2012 10:20:16 ******/
ALTER TABLE [dbo].[ShowReactionSummary] ADD  CONSTRAINT [DF_ShowSummary_Total]  DEFAULT ((0)) FOR [Total]
GO
/****** Object:  Default [DF_ShowSummary_Total7Days]    Script Date: 08/24/2012 10:20:16 ******/
ALTER TABLE [dbo].[ShowReactionSummary] ADD  CONSTRAINT [DF_ShowSummary_Total7Days]  DEFAULT ((0)) FOR [Total7Days]
GO
/****** Object:  Default [DF_ShowSummary_Total30Days]    Script Date: 08/24/2012 10:20:16 ******/
ALTER TABLE [dbo].[ShowReactionSummary] ADD  CONSTRAINT [DF_ShowSummary_Total30Days]  DEFAULT ((0)) FOR [Total30Days]
GO


/****** Object:  Trigger [dbo].[CelebrityReactionDeleteTrigger]    Script Date: 08/24/2012 10:21:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create trigger [dbo].[CelebrityReactionDeleteTrigger]
on [dbo].[CelebrityReactions]
after DELETE as
begin
    declare @celebrityId int
    declare @reactionTypeId int
    declare @date datetime
    declare @adjust7Days int
    declare @adjust30Days int
    select @celebrityId=CelebrityId, @reactionTypeId=ReactionTypeId, @date=DateTime from deleted
    if @date > DATEADD(d,-7,GETDATE())
      set @adjust7days=1
    else 
      set @adjust7Days=0
    if @date > DATEADD(d,-30,GETDATE())
      set @adjust30Days=1
    else
      set @adjust30Days=0
      
    update CelebrityReactionSummary 
    set Total = Total-1, Total7Days = Total7Days-@adjust7Days, Total30Days=Total30Days-@adjust30Days
    where CelebrityId=@celebrityId and ReactionTypeId=@reactionTypeId
end


/****** Object:  Trigger [dbo].[CelebrityReactionInsertTrigger]    Script Date: 08/24/2012 10:21:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create trigger [dbo].[CelebrityReactionInsertTrigger]
on [dbo].[CelebrityReactions]
after INSERT as
begin
    declare @celebrityId int
    declare @reactionTypeId int
    select @celebrityId=CelebrityId, @reactionTypeId=ReactionTypeId from inserted
    update CelebrityReactionSummary 
    set Total = Total+1, Total7Days = Total7Days+1, Total30Days = Total30Days+1
    where CelebrityId=@celebrityId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into CelebrityReactionSummary (celebrityId, ReactionTypeId, Total, Total7Days, Total30Days)
         values (@celebrityId,@reactionTypeId,1,1,1)

end

/****** Object:  Trigger [dbo].[EpisodeReactionDeleteTrigger]    Script Date: 08/24/2012 10:22:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create trigger [dbo].[EpisodeReactionDeleteTrigger]
on [dbo].[EpisodeReactions]
after DELETE as
begin
    declare @EpisodeId int
    declare @reactionTypeId int
    declare @date datetime
    declare @adjust7Days int
    declare @adjust30Days int

    select @EpisodeId=EpisodeId, @reactionTypeId=ReactionTypeId, @date=DateTime from deleted
    
    if @date > DATEADD(d,-7,GETDATE())
      set @adjust7days=1
    else 
      set @adjust7Days=0
    if @date > DATEADD(d,-30,GETDATE())
      set @adjust30Days=1
    else
      set @adjust30Days=0

    update EpisodeReactionSummary 
    set Total = Total-1, Total7Days = Total7Days-@adjust7Days, Total30Days=Total30Days-@adjust30Days
    where EpisodeId=@EpisodeId and ReactionTypeId=@reactionTypeId
end


/****** Object:  Trigger [dbo].[EpisodeReactionInsertTrigger]    Script Date: 08/24/2012 10:23:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create trigger [dbo].[EpisodeReactionInsertTrigger]
on [dbo].[EpisodeReactions]
after INSERT as
begin
    declare @episodeId int
    declare @reactionTypeId int
    select @episodeId=EpisodeId, @reactionTypeId=ReactionTypeId from inserted
    update EpisodeReactionSummary 
    set Total = Total+1, Total7Days = Total7Days+1, Total30Days = Total30Days+1
    where EpisodeId=@episodeId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into EpisodeReactionSummary (EpisodeId, ReactionTypeId, Total, Total7Days, Total30Days)
         values (@episodeId,@reactionTypeId,1,1,1)

end

/****** Object:  Trigger [dbo].[ShowReactionDeleteTrigger]    Script Date: 08/24/2012 10:23:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create trigger [dbo].[ShowReactionDeleteTrigger]
on [dbo].[ShowReactions]
after DELETE as
begin
    declare @categoryId int
    declare @reactionTypeId int
    declare @date datetime
    declare @adjust7Days int
    declare @adjust30Days int

    select @categoryId=categoryId, @reactionTypeId=ReactionTypeId, @date=DateTime from deleted
    
    if @date > DATEADD(d,-7,GETDATE())
      set @adjust7days=1
    else 
      set @adjust7Days=0
    if @date > DATEADD(d,-30,GETDATE())
      set @adjust30Days=1
    else
      set @adjust30Days=0

    update ShowReactionSummary 
    set Total = Total-1, Total7Days = Total7Days-@adjust7Days, Total30Days=Total30Days-@adjust30Days
    where categoryId=@categoryId and ReactionTypeId=@reactionTypeId
end

/****** Object:  Trigger [dbo].[ShowReactionInsertTrigger]    Script Date: 08/24/2012 10:24:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create trigger [dbo].[ShowReactionInsertTrigger]
on [dbo].[ShowReactions]
after INSERT as
begin
    declare @categoryId int
    declare @reactionTypeId int
    select @categoryId=categoryId, @reactionTypeId=ReactionTypeId from inserted
    update ShowReactionSummary 
    set Total = Total+1, Total7Days = Total7Days+1, Total30Days = Total30Days+1
    where categoryId=@categoryId and ReactionTypeId=@reactionTypeId
    if @@ROWCOUNT = 0
       insert into ShowReactionSummary (categoryId, ReactionTypeId, Total, Total7Days, Total30Days)
         values (@categoryId,@reactionTypeId,1,1,1)

end